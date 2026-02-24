<template>
  <section>
    <h2>Concepts</h2>
    <ul>
      <li><code>Emcode.Pst.Application</code>: facade utama.</li>
      <li><code>Emcode.Pst.Application.Abstractions</code>: kontrak reader/writer.</li>
      <li><code>Emcode.Pst.Domain</code>: model folder, message, attachment, recipient.</li>
      <li><code>Emcode.Pst.Infrastructure</code>: implementasi parser/writer.</li>
    </ul>
    <p>Semua API async mendukung <code>CancellationToken</code>.</p>
  </section>
</template>
