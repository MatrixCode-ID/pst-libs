<script setup>
import { apiNamespaces } from "../data/apiObjects";
</script>

<template>
  <section>
    <h2>Namespaces</h2>
    <div v-for="ns in apiNamespaces" :key="ns.id" class="ns-card">
      <h3>{{ ns.id }}</h3>
      <p>Namespace API publik.</p>
      <RouterLink :to="`/api/namespace/${encodeURIComponent(ns.id)}`">Lihat detail</RouterLink>
    </div>
  </section>
</template>

<style scoped>
.ns-card {
  border: 1px solid var(--doc-border);
  border-radius: 10px;
  padding: 12px;
  margin-bottom: 10px;
}
</style>
