<template>
  <div class="docs-root">
    <aside class="docs-sidebar">
      <h2>Documentation</h2>
      <nav>
        <RouterLink to="/">Home</RouterLink>
        <RouterLink to="/toc">TOC</RouterLink>
        <div class="nav-group">
          <span>Help</span>
          <RouterLink to="/help">Overview</RouterLink>
          <RouterLink to="/help/getting-started">Getting Started</RouterLink>
          <RouterLink to="/help/concepts">Concepts</RouterLink>
          <RouterLink to="/help/how-to/open-and-read">How-To: Open and Read</RouterLink>
          <RouterLink to="/help/how-to/create-folder-and-message">How-To: Create Folder</RouterLink>
          <RouterLink to="/help/how-to/import-eml">How-To: Import EML</RouterLink>
          <RouterLink to="/help/faq">FAQ</RouterLink>
        </div>
        <div class="nav-group">
          <span>API References</span>
          <RouterLink to="/api">Overview</RouterLink>
          <RouterLink to="/api/namespaces">Namespaces</RouterLink>
        </div>
      </nav>
    </aside>
    <main class="docs-content">
      <header class="docs-header">
        <h1>Emcode.Pst Docs (Vue)</h1>
        <button type="button" class="theme-btn" @click="toggleTheme">
          {{ isDark ? "Light mode" : "Dark mode" }}
        </button>
      </header>
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";

const themeKey = "doc-theme";
const isDark = ref(false);

const currentTheme = computed(() => (isDark.value ? "dark" : "light"));

function applyTheme(theme) {
  document.documentElement.setAttribute("data-theme", theme);
  localStorage.setItem(themeKey, theme);
  isDark.value = theme === "dark";
}

function toggleTheme() {
  applyTheme(currentTheme.value === "dark" ? "light" : "dark");
}

onMounted(() => {
  const saved = localStorage.getItem(themeKey);
  const prefersDark = window.matchMedia?.("(prefers-color-scheme: dark)").matches;
  const theme = saved === "dark" || saved === "light" ? saved : (prefersDark ? "dark" : "light");
  applyTheme(theme);
});
</script>
