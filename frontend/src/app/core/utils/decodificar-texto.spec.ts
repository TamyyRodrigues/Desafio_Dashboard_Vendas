import { decodificarTexto } from './decodificar-texto';

describe('decodificarTexto', () => {
  it('lê UTF-8 corretamente', () => {
    const bytes = new TextEncoder().encode('Calça,Tênis');

    expect(decodificarTexto(bytes.buffer)).toBe('Calça,Tênis');
  });

  it('lê Windows-1252 (ANSI) quando o arquivo não é UTF-8', () => {
    // "Calça,Tênis" em ANSI: ç = 0xE7, ê = 0xEA
    const bytes = new Uint8Array([0x43, 0x61, 0x6c, 0xe7, 0x61, 0x2c, 0x54, 0xea, 0x6e, 0x69, 0x73]);

    expect(decodificarTexto(bytes.buffer)).toBe('Calça,Tênis');
  });
});
