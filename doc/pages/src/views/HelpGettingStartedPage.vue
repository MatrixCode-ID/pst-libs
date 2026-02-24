<template>
  <section>
    <h2>Getting Started</h2>
    <h3>Prasyarat</h3>
    <ul>
      <li>.NET 10 SDK</li>
      <li>Package <code>Emcode.Pst.Libs</code></li>
    </ul>
    <h3>Quick Start (Read)</h3>
    <pre><code class="language-csharp">using Emcode.Pst.Application;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = false
});</code></pre>
  </section>
</template>
