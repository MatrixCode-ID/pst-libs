import fs from "node:fs";
import path from "node:path";

const docsRoot = path.resolve(process.cwd());
const sourceRoot = path.resolve(docsRoot, "..", "..", "src", "Emcode.Pst.Libs");
const outputFile = path.join(docsRoot, "src", "data", "apiObjects.js");

function collectCsFiles(dir) {
  const files = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      files.push(...collectCsFiles(fullPath));
      continue;
    }
    if (entry.name.endsWith(".cs")) {
      files.push(fullPath);
    }
  }
  return files;
}

function stripXmlTags(text) {
  return text
    .replace(/<see cref="([^"]+)"\s*\/>/g, "$1")
    .replace(/<\/?c>/g, "")
    .replace(/<\/?para>/g, "")
    .replace(/<\/?summary>/g, "")
    .trim();
}

function sanitizeLine(line) {
  return stripXmlTags(line.replace(/^\s*\/\/\/\s?/, "").trim());
}

function parseFile(filePath) {
  const lines = fs.readFileSync(filePath, "utf8").split(/\r?\n/);

  let namespaceName = "";
  let pendingSummary = "";
  let summaryBuffer = [];
  let inSummary = false;

  let currentType = null;
  let braceDepth = 0;

  for (const rawLine of lines) {
    const line = rawLine.trim();

    const namespaceMatch = line.match(/^namespace\s+([A-Za-z0-9_.]+);/);
    if (namespaceMatch) {
      namespaceName = namespaceMatch[1];
    }

    if (line.startsWith("/// <summary>")) {
      inSummary = true;
      summaryBuffer = [];
      const cleaned = sanitizeLine(line);
      if (cleaned) {
        summaryBuffer.push(cleaned);
      }
      if (line.includes("</summary>")) {
        inSummary = false;
        pendingSummary = summaryBuffer.join(" ").trim();
      }
      continue;
    }

    if (inSummary) {
      const cleaned = sanitizeLine(line);
      if (cleaned) {
        summaryBuffer.push(cleaned);
      }
      if (line.includes("</summary>")) {
        inSummary = false;
        pendingSummary = summaryBuffer.join(" ").trim();
      }
      continue;
    }

    if (!currentType) {
      const typeMatch = line.match(/^public\s+(?:sealed\s+|static\s+|readonly\s+|abstract\s+|partial\s+)*(class|interface|enum|struct)\s+([A-Za-z_]\w*)(?:\s*:\s*.+)?/);
      if (!typeMatch) {
        continue;
      }

      const kind = typeMatch[1];
      const typeName = typeMatch[2];
      const signature = rawLine.trim();
      currentType = {
        namespace: namespaceName,
        type: typeName,
        summary: pendingSummary,
        signature,
        kind: kind === "enum" ? "enum" : "type",
        constructors: [],
        properties: [],
        methods: [],
        events: [],
        fields: []
      };
      pendingSummary = "";

      braceDepth += (rawLine.match(/{/g) || []).length;
      braceDepth -= (rawLine.match(/}/g) || []).length;
      continue;
    }

    const openCount = (rawLine.match(/{/g) || []).length;
    const closeCount = (rawLine.match(/}/g) || []).length;

    const atMemberLevel = braceDepth === 1;

    if (atMemberLevel && currentType.kind === "enum") {
      const enumMatch = line.match(/^([A-Za-z_]\w*)\s*=\s*([^,]+),?/);
      if (enumMatch) {
        currentType.fields.push({
          name: enumMatch[1],
          value: enumMatch[2].trim(),
          description: pendingSummary
        });
        pendingSummary = "";
      }
    } else if (atMemberLevel) {
      const ctorMatch = line.match(new RegExp(`^public\\s+${currentType.type}\\s*\\((.*)\\)`));
      if (ctorMatch) {
        currentType.constructors.push({
          signature: `${currentType.type}(${ctorMatch[1].trim()})`,
          description: pendingSummary
        });
        pendingSummary = "";
      } else {
        const propPublicMatch = line.match(/^public\s+(.+?)\s+([A-Za-z_]\w*)\s*\{\s*get;/);
        const propInterfaceMatch = currentType.signature.includes("interface")
          ? line.match(/^(.+?)\s+([A-Za-z_]\w*)\s*\{\s*get;/)
          : null;
        const propMatch = propPublicMatch || propInterfaceMatch;
        if (propMatch) {
          currentType.properties.push({
            name: propMatch[2].trim(),
            type: propMatch[1].trim(),
            description: pendingSummary
          });
          pendingSummary = "";
        } else {
          const eventMatch = line.match(/^public\s+event\s+(.+?)\s+([A-Za-z_]\w*)\s*;/);
          if (eventMatch) {
            currentType.events.push({
              name: eventMatch[2].trim(),
              type: eventMatch[1].trim(),
              description: pendingSummary
            });
            pendingSummary = "";
          } else {
            const methodPublicMatch = line.match(/^public\s+(?:static\s+|virtual\s+|override\s+|sealed\s+|abstract\s+|async\s+)*(?<ret>[^(){};]+?)\s+(?<name>[A-Za-z_]\w*)\s*\((?<params>.*)\)/);
            const methodInterfaceMatch = currentType.signature.includes("interface")
              ? line.match(/^(?<ret>[^(){};]+?)\s+(?<name>[A-Za-z_]\w*)\s*\((?<params>.*)\)\s*;/)
              : null;
            const methodMatch = methodPublicMatch || methodInterfaceMatch;
            if (methodMatch && methodMatch.groups.name !== currentType.type) {
              const methodSignature = `${methodMatch.groups.name}(${methodMatch.groups.params.trim()})`;
              currentType.methods.push({
                signature: methodSignature,
                returnType: methodMatch.groups.ret.trim(),
                description: pendingSummary
              });
              pendingSummary = "";
            }
          }
        }
      }
    }

    braceDepth += openCount;
    braceDepth -= closeCount;

    if (braceDepth <= 0 && currentType) {
      return currentType;
    }
  }

  return currentType;
}

const items = collectCsFiles(sourceRoot)
  .map(parseFile)
  .filter((item) => item && item.namespace && item.type && /^Emcode\.Pst\./.test(item.namespace))
  .sort((a, b) => {
    const ns = a.namespace.localeCompare(b.namespace);
    if (ns !== 0) {
      return ns;
    }
    return a.type.localeCompare(b.type);
  });

const namespaceMap = new Map();
for (const item of items) {
  if (!namespaceMap.has(item.namespace)) {
    namespaceMap.set(item.namespace, []);
  }
  namespaceMap.get(item.namespace).push(item.type);
}

const apiNamespaces = [...namespaceMap.entries()]
  .map(([id, types]) => ({ id, types: [...types].sort((a, b) => a.localeCompare(b)) }))
  .sort((a, b) => a.id.localeCompare(b.id));

const output = `export const apiObjects = ${JSON.stringify(items, null, 2)};

export const apiNamespaces = ${JSON.stringify(apiNamespaces, null, 2)};

export function findApiObject(namespaceName, typeName) {
  return apiObjects.find((item) => item.namespace === namespaceName && item.type === typeName);
}
`;

fs.mkdirSync(path.dirname(outputFile), { recursive: true });
fs.writeFileSync(outputFile, output, "utf8");

console.log(`Generated ${items.length} API objects from source code -> ${path.relative(docsRoot, outputFile)}`);
