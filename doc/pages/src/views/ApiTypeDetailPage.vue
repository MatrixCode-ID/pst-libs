<script setup>
import { computed } from "vue";
import { findApiObject } from "../data/apiObjects";

const props = defineProps({
  namespace: {
    type: String,
    required: true
  },
  type: {
    type: String,
    required: true
  }
});

const namespaceName = computed(() => decodeURIComponent(props.namespace));
const typeName = computed(() => decodeURIComponent(props.type));
const item = computed(() => findApiObject(namespaceName.value, typeName.value));
</script>

<template>
  <section v-if="item">
    <h2>{{ item.type }}</h2>
    <p>Namespace: <code>{{ item.namespace }}</code></p>
    <p>{{ item.summary }}</p>

    <h3>Definition</h3>
    <pre><code class="language-csharp">{{ item.signature }}</code></pre>

    <template v-if="item.kind === 'enum'">
      <h3>Fields</h3>
      <table>
        <thead>
          <tr>
            <th>Field</th>
            <th>Value</th>
            <th>Keterangan</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="field in item.fields" :key="field.name">
            <td><code>{{ field.name }}</code></td>
            <td><code>{{ field.value }}</code></td>
            <td>{{ field.description }}</td>
          </tr>
        </tbody>
      </table>
    </template>

    <template v-else>
      <h3>Constructors</h3>
      <table>
        <thead>
          <tr>
            <th>Constructor</th>
            <th>Keterangan</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!item.constructors.length">
            <td colspan="2">Tidak ada constructor publik.</td>
          </tr>
          <tr v-for="ctor in item.constructors" :key="ctor.signature">
            <td><code>{{ ctor.signature }}</code></td>
            <td>{{ ctor.description }}</td>
          </tr>
        </tbody>
      </table>

      <h3>Properties</h3>
      <table>
        <thead>
          <tr>
            <th>Property</th>
            <th>Type</th>
            <th>Keterangan</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!item.properties.length">
            <td colspan="3">Tidak ada property publik.</td>
          </tr>
          <tr v-for="prop in item.properties" :key="`${prop.name}-${prop.type}`">
            <td><code>{{ prop.name }}</code></td>
            <td><code>{{ prop.type }}</code></td>
            <td>{{ prop.description }}</td>
          </tr>
        </tbody>
      </table>

      <h3>Methods</h3>
      <table>
        <thead>
          <tr>
            <th>Method</th>
            <th>Return Type</th>
            <th>Keterangan</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!item.methods.length">
            <td colspan="3">Tidak ada method publik.</td>
          </tr>
          <tr v-for="method in item.methods" :key="method.signature">
            <td><code>{{ method.signature }}</code></td>
            <td><code>{{ method.returnType }}</code></td>
            <td>{{ method.description }}</td>
          </tr>
        </tbody>
      </table>

      <h3>Events</h3>
      <table>
        <thead>
          <tr>
            <th>Event</th>
            <th>Type</th>
            <th>Keterangan</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!item.events.length">
            <td colspan="3">Tidak ada event publik.</td>
          </tr>
          <tr v-for="event in item.events" :key="event.name">
            <td><code>{{ event.name }}</code></td>
            <td><code>{{ event.type }}</code></td>
            <td>{{ event.description }}</td>
          </tr>
        </tbody>
      </table>
    </template>
  </section>

  <section v-else>
    <h2>Object API tidak ditemukan</h2>
    <p>Namespace/type tidak tersedia di dataset migrasi API.</p>
  </section>
</template>
