import { inject, Injectable } from '@angular/core';
import { ConfigService } from '../config.service';
import * as CryptoJS from 'crypto-js';


@Injectable({
  providedIn: 'root'
})
export class EncryptionService {
  private config = inject(ConfigService);

  constructor() { }

  encryptAES_JSON(data: any): string {
    // Convert the object to a string first
    const stringData = JSON.stringify(data);

    const keyBytes = this.getKeyBytes(this.config.encryptionKey, 32);
    const key = CryptoJS.enc.Utf8.parse(keyBytes);

    // Zero IV (16 bytes of zeros) - Must match C#'s new byte[16]
    const iv = CryptoJS.lib.WordArray.create(new Array(16).fill(0));

    // Encrypt with AES-CBC, PKCS7 padding
    const encrypted = CryptoJS.AES.encrypt(stringData, key, {
      iv: iv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });

    // Return Base64 string
    return encrypted.toString();
  }


    private getKeyBytes(keyString: string, requiredSize: number): string {
    // This mimics the C# GetKey method logic
    // If key is longer than required, truncate it
    if (keyString.length > requiredSize) {
      return keyString.substring(0, requiredSize);
    }

    // If key is shorter than required, pad it (C# might pad differently)
    // For now, just return as is - C# GetKey should handle padding
    return keyString;
  }
  decryptAES(data: string): string {
    // Key bytes must be derived the same way as in the encryption function
    const keyBytes = this.getKeyBytes(this.config.encryptionKey, 32);
    const key = CryptoJS.enc.Utf8.parse(keyBytes);

    // Zero IV (16 bytes of zeros) - Must match C#'s new byte[16]
    const iv = CryptoJS.lib.WordArray.create(new Array(16).fill(0));

    // Decrypt the ciphertext (which is a Base64 string from encrypted.toString())
    const decrypted = CryptoJS.AES.decrypt(data, key, {
      iv: iv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });

    // Convert the decrypted WordArray to a UTF-8 string
    return decrypted.toString(CryptoJS.enc.Utf8);
  }

}
