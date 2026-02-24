export const namespaces = [
  {
    id: "Emcode.Pst.Application",
    description: "Facade penggunaan library.",
    types: ["PstFile", "PstOpenOptions"]
  },
  {
    id: "Emcode.Pst.Application.Abstractions",
    description: "Kontrak read/write agar implementasi dapat dipertukarkan.",
    types: ["IPstReader", "IPstWriter", "IPstWriterWithContext", "PstReadResult", "PstWriteContext"]
  },
  {
    id: "Emcode.Pst.Domain",
    description: "Model domain pesan, folder, recipient, attachment.",
    types: [
      "PstAttachment",
      "PstCryptMethod",
      "PstDraftAttachment",
      "PstDraftRecipient",
      "PstFolder",
      "PstFormat",
      "PstHeaderInfo",
      "PstMessage",
      "PstMessageDraft",
      "PstRecipient",
      "PstRecipientType"
    ]
  },
  {
    id: "Emcode.Pst.Infrastructure",
    description: "Implementasi reader/writer PST.",
    types: ["PstInMemoryWriter", "PstMinimalReader", "PstNdbReader", "PstNdbWriter"]
  }
];
