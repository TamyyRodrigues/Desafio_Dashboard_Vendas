/**
 * Decodifica os bytes de um arquivo de texto.
 * Tenta UTF-8 estrito; se houver bytes inválidos (ex.: CSV salvo em ANSI pelo Excel),
 * usa Windows-1252, que preserva ç, ã, é, ê etc.
 */
export function decodificarTexto(buffer: ArrayBuffer): string {
  try {
    return new TextDecoder('utf-8', { fatal: true }).decode(buffer);
  } catch {
    return new TextDecoder('windows-1252').decode(buffer);
  }
}
