<template>
  <section>
    <h2>FAQ</h2>
    <h3>Apakah library ini hanya read-only?</h3>
    <p>Tidak. Read tersedia, write tersedia lewat kontrak writer (<code>IPstWriter</code>).</p>
    <h3>Bedanya <code>PstMinimalReader</code> dan <code>PstNdbReader</code>?</h3>
    <ul>
      <li><code>PstMinimalReader</code>: validasi header + metadata dasar.</li>
      <li><code>PstNdbReader</code>: parsing NDB untuk folder/message nyata.</li>
    </ul>
  </section>
</template>
