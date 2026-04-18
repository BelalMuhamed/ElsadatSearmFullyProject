import { Injectable } from '@angular/core';
import Swal, { SweetAlertOptions } from 'sweetalert2';
import { Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class SwalService {

  private isDark(): boolean {
    return document.body.classList.contains('dark-mode');
  }

  private getThemeOptions(): SweetAlertOptions {
    const isDark = this.isDark();

    return {
      background: isDark ? '#1a1a1a' : '#ffffff',
      color: isDark ? '#ffffff' : '#000000',
      confirmButtonColor: 'var(--gold)',
      cancelButtonColor: '#6b7280',
      customClass: {
        popup: 'custom-swal-popup'
      }
    };
  }

  // ✅ Success
  success(message: string = 'Operation done successfully') {
    return Swal.fire({
      ...this.getThemeOptions(),
      icon: 'success',
      title: message
    });
  }

  // ❌ Error
  error(message: string = 'Something went wrong') {
    return Swal.fire({
      ...this.getThemeOptions(),
      icon: 'error',
      title: message
    });
  }

  // ⚠️ Warning
  warning(message: string) {
    return Swal.fire({
      ...this.getThemeOptions(),
      icon: 'warning',
      title: message
    });
  }

  // ℹ️ Info
  info(message: string) {
    return Swal.fire({
      ...this.getThemeOptions(),
      icon: 'info',
      title: message
    });
  }

  // ❓ Confirm
  confirm(message: string = 'Are you sure?') {
    return Swal.fire({
      ...this.getThemeOptions(),
      icon: 'question',
      title: message,
      showCancelButton: true,
      confirmButtonText: 'Yes',
      cancelButtonText: 'Cancel'
    });
  }

  // 🔥 Handle Result<T>
  handleResponse<T>(result: Result<T>) {
    if (result.isSuccess) {
      return this.success(result.message || 'Success');
    } else {
      return this.error(result.message || 'Failed');
    }
  }
}
