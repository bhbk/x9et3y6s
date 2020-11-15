import { Injectable } from '@angular/core';
import { NotificationService } from '@progress/kendo-angular-notification';

@Injectable({
  providedIn: 'root'
})
export class NotifyService {
  private disposeAfter = 10000;

  constructor(private notificationService: NotificationService) { }

  showInfo(message: string) {
    this.notificationService.show({
      content: message,
      type: { style: 'info', icon: true },
      position: { horizontal: 'center', vertical: 'top' }
    });
  }

  showSuccess(message: string) {
    this.notificationService.show({
      content: message,
      type: { style: 'success', icon: true },
      position: { horizontal: 'center', vertical: 'top' },
      hideAfter: this.disposeAfter
    });
  }

  showWarning(message: string) {
    this.notificationService.show({
      content: message,
      type: { style: 'warning', icon: true },
      position: { horizontal: 'center', vertical: 'top' }
    });
  }

  showError(message: string) {
    this.notificationService.show({
      content: message,
      type: { style: 'error', icon: true },
      position: { horizontal: 'center', vertical: 'top' },
      closable: true
    });
  }
}
