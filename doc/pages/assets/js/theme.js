(function () {
  var storageKey = "doc-theme";
  var root = document.documentElement;
  var prefersDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
  var saved = localStorage.getItem(storageKey);
  var theme = (saved === "dark" || saved === "light") ? saved : (prefersDark ? "dark" : "light");

  function syncPrismTheme(value) {
    var prismLink = document.querySelector('link[href*="prismjs"][href*="/themes/"]');
    if (!prismLink) {
      return;
    }

    var href = prismLink.getAttribute("href") || "";
    var baseMatch = href.match(/^(.*\/themes\/)/);
    var base = baseMatch ? baseMatch[1] : "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/";
    var themeFile = value === "dark" ? "prism-okaidia.min.css" : "prism.min.css";

    prismLink.setAttribute("href", base + themeFile);
  }

  function applyTheme(value) {
    root.setAttribute("data-theme", value);
    localStorage.setItem(storageKey, value);
    syncPrismTheme(value);

    var button = document.getElementById("doc-theme-toggle");
    if (button) {
      button.textContent = value === "dark" ? "Light mode" : "Dark mode";
      button.setAttribute("aria-label", "Switch theme to " + (value === "dark" ? "light" : "dark"));
    }
  }

  function addToggle() {
    var main = document.querySelector("main");
    if (!main || document.getElementById("doc-theme-toggle")) {
      return;
    }

    var bar = document.createElement("div");
    bar.className = "doc-theme-bar";

    var button = document.createElement("button");
    button.id = "doc-theme-toggle";
    button.type = "button";
    button.addEventListener("click", function () {
      var next = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
      applyTheme(next);
    });

    bar.appendChild(button);
    main.insertBefore(bar, main.firstChild);
  }

  function normalizePath(path) {
    if (!path) {
      return "";
    }

    return path.replace(/\/+$/, "");
  }

  function getDocsRootPath() {
    var script = document.currentScript;
    if (!script) {
      var scripts = document.querySelectorAll("script[src]");
      for (var i = scripts.length - 1; i >= 0; i--) {
        var src = scripts[i].getAttribute("src") || "";
        if (src.indexOf("assets/js/theme.js") !== -1) {
          script = scripts[i];
          break;
        }
      }
    }

    if (!script || !script.src) {
      return "";
    }

    var absolute = new URL(script.src, window.location.href);
    return absolute.pathname.replace(/\/assets\/js\/theme\.js$/, "");
  }

  function createNavLink(href, text, currentPath) {
    var item = document.createElement("li");
    var link = document.createElement("a");
    link.href = href;
    link.textContent = text;

    var targetPath = normalizePath(new URL(href, window.location.origin).pathname);
    var active = currentPath === targetPath;
    if (!active) {
      if (targetPath === normalizePath(getDocsRootPath() + "/api/index.html") && currentPath.indexOf(normalizePath(getDocsRootPath() + "/api/")) === 0) {
        active = true;
      }
      if (targetPath === normalizePath(getDocsRootPath() + "/help/index.html") && currentPath.indexOf(normalizePath(getDocsRootPath() + "/help/")) === 0) {
        active = true;
      }
    }

    if (active) {
      link.classList.add("is-active");
    }

    item.appendChild(link);
    return item;
  }

  function injectGlobalSidebar() {
    if (document.querySelector(".doc-sidebar")) {
      document.body.classList.add("docs-layout");
      return;
    }

    var main = document.querySelector("main");
    if (!main) {
      return;
    }

    var docsRoot = getDocsRootPath();
    if (!docsRoot) {
      return;
    }

    var currentPath = normalizePath(window.location.pathname);
    var shell = document.createElement("div");
    shell.className = "doc-shell";

    var sidebar = document.createElement("aside");
    sidebar.className = "doc-sidebar";
    sidebar.setAttribute("aria-label", "Documentation Navigation");

    var heading = document.createElement("h2");
    heading.textContent = "Documentation";
    sidebar.appendChild(heading);

    var rootList = document.createElement("ul");
    rootList.appendChild(createNavLink(docsRoot + "/index.html", "Home", currentPath));
    rootList.appendChild(createNavLink(docsRoot + "/toc.html", "TOC", currentPath));
    sidebar.appendChild(rootList);

    var helpGroup = document.createElement("ul");
    helpGroup.className = "nav-group";
    helpGroup.appendChild(createNavLink(docsRoot + "/help/index.html", "Help Overview", currentPath));
    helpGroup.appendChild(createNavLink(docsRoot + "/help/getting-started.html", "Getting Started", currentPath));
    helpGroup.appendChild(createNavLink(docsRoot + "/help/concepts.html", "Concepts", currentPath));
    helpGroup.appendChild(createNavLink(docsRoot + "/help/how-to/open-and-read.html", "How-To: Open and Read", currentPath));
    helpGroup.appendChild(createNavLink(docsRoot + "/help/how-to/create-folder-and-message.html", "How-To: Create Folder", currentPath));
    helpGroup.appendChild(createNavLink(docsRoot + "/help/how-to/import-eml.html", "How-To: Import EML", currentPath));
    helpGroup.appendChild(createNavLink(docsRoot + "/help/faq.html", "FAQ", currentPath));
    sidebar.appendChild(helpGroup);

    var apiGroup = document.createElement("ul");
    apiGroup.className = "nav-group";
    apiGroup.appendChild(createNavLink(docsRoot + "/api/index.html", "API Overview", currentPath));
    apiGroup.appendChild(createNavLink(docsRoot + "/api/namespaces.html", "Namespaces", currentPath));
    apiGroup.appendChild(createNavLink(docsRoot + "/api/Emcode.Pst.Application/index.html", "Emcode.Pst.Application", currentPath));
    apiGroup.appendChild(createNavLink(docsRoot + "/api/Emcode.Pst.Application.Abstractions/index.html", "Emcode.Pst.Application.Abstractions", currentPath));
    apiGroup.appendChild(createNavLink(docsRoot + "/api/Emcode.Pst.Domain/index.html", "Emcode.Pst.Domain", currentPath));
    apiGroup.appendChild(createNavLink(docsRoot + "/api/Emcode.Pst.Infrastructure/index.html", "Emcode.Pst.Infrastructure", currentPath));
    sidebar.appendChild(apiGroup);

    main.classList.add("doc-content");
    document.body.classList.add("docs-layout");

    var parent = main.parentNode;
    parent.insertBefore(shell, main);
    shell.appendChild(sidebar);
    shell.appendChild(main);
  }

  applyTheme(theme);

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", function () {
      injectGlobalSidebar();
      addToggle();
      applyTheme(root.getAttribute("data-theme") || theme);
    });
  } else {
    injectGlobalSidebar();
    addToggle();
    applyTheme(root.getAttribute("data-theme") || theme);
  }
})();
