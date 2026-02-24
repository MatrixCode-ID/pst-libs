<script setup>
import { computed } from "vue";
import { namespaces } from "../data/docs";

const props = defineProps({
  name: {
    type: String,
    required: true
  }
});

const namespaceName = computed(() => decodeURIComponent(props.name));
const namespaceData = computed(() => namespaces.find((x) => x.id === namespaceName.value));
</script>

<template>
  <section v-if="namespaceData">
    <h2>{{ namespaceData.id }}</h2>
    <p>{{ namespaceData.description }}</p>
    <h3>Types</h3>
    <ul>
      <li v-for="type in namespaceData.types" :key="type">{{ type }}</li>
    </ul>
    <p>
      Catatan: Migrasi ke Vue sedang tahap awal. Halaman detail type akan dipindahkan bertahap dari versi HTML legacy.
    </p>
  </section>
  <section v-else>
    <h2>Namespace tidak ditemukan</h2>
    <p>Pastikan path namespace benar.</p>
  </section>
</template>
