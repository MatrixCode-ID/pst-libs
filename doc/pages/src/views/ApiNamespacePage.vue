<script setup>
import { computed } from "vue";
import { apiNamespaces } from "../data/apiObjects";

const props = defineProps({
  name: {
    type: String,
    required: true
  }
});

const namespaceName = computed(() => decodeURIComponent(props.name));
const namespaceData = computed(() => apiNamespaces.find((x) => x.id === namespaceName.value));
</script>

<template>
  <section v-if="namespaceData">
    <h2>{{ namespaceData.id }}</h2>
    <p>Namespace API publik.</p>
    <h3>Types</h3>
    <ul>
      <li v-for="type in namespaceData.types" :key="type">
        <RouterLink :to="`/api/type/${encodeURIComponent(namespaceData.id)}/${encodeURIComponent(type)}`">
          {{ type }}
        </RouterLink>
      </li>
    </ul>
  </section>
  <section v-else>
    <h2>Namespace tidak ditemukan</h2>
    <p>Pastikan path namespace benar.</p>
  </section>
</template>
