import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root',
})

export class NotificationService {
  private lastMessage: string | null = null;
  private lastMessageTime: number = 0;
  private timeLimit: number = 2000; // 2 seconds

  constructor(private messageService: MessageService) {}

  error(message: string): void {
    const currentTime = Date.now();
    if (
      this.lastMessage &&
      this.lastMessage === message &&
      currentTime - this.lastMessageTime < this.timeLimit
    ) {
      // Prevent duplicate message
      return;
    }

    this.lastMessage = message;
    this.lastMessageTime = currentTime;
    this.messageService.add({detail: message, summary: 'error'});
  }

  success(message: string): void {
    this.messageService.add({detail: message, summary: 'success'});
  }

  warning(message: string): void {
    this.messageService.add({detail: message, summary: 'warning'});
  }


  clear(): void {
    this.messageService.clear();
  }
}
