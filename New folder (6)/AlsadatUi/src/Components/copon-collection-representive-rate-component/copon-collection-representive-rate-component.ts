import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CoponCollectionRepresentiveRateService } from '../../app/Services/copon-collection-representive-rate.service';
import Swal from 'sweetalert2';
import { CoponRateDialogComponent } from './copon-rate-dialog/copon-rate-dialog.component';

@Component({
  selector: 'app-copon-collection-rate',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatDialogModule, MatCardModule, MatSlideToggleModule, MatTooltipModule],
  templateUrl: './copon-collection-representive-rate-component.html',
  styleUrls: ['./copon-collection-representive-rate-component.css']
})
export class CoponCollectionRepresentiveRateComponent implements OnInit {
  private service = inject(CoponCollectionRepresentiveRateService);
  private dialog = inject(MatDialog);

  items: any[] = [];
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    this.service.getAll(this.currentPage, this.pageSize).subscribe({ next: (res:any) => { if (res?.items && Array.isArray(res.items)) { this.items = res.items; this.totalCount = res.totalCount ?? res.items.length; } else { Swal.fire('خطأ', res?.message ?? 'فشل التحميل', 'error'); } this.isLoading = false; }, error: (e) => { this.isLoading = false; Swal.fire('خطأ', e?.error?.message ?? 'حدث خطأ', 'error'); } });
  }

  openAdd(): void { const ref = this.dialog.open(CoponRateDialogComponent, { data: null, width: '480px' }); ref.afterClosed().subscribe((saved:any) => { if (saved) this.load(); }); }
  openEdit(item:any): void { const ref = this.dialog.open(CoponRateDialogComponent, { data: item, width: '480px' }); ref.afterClosed().subscribe((saved:any) => { if (saved) this.load(); }); }

  toggleStatus(item:any): void { if (!item) return; if (item.isDeleted) { this.service.restore(item).subscribe({ next: () => { Swal.fire('تم', 'تم الاستعادة', 'success'); this.load(); }, error: e => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error') }); } else { this.service.softDelete(item).subscribe({ next: () => { Swal.fire('تم', 'تم الحذف', 'success'); this.load(); }, error: e => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error') }); } }

  hardDelete(item:any): void { Swal.fire({ title: 'تأكيد', text: `هل تريد حذف السجل نهائيا؟`, icon: 'warning', showCancelButton: true }).then(r => { if (r.isConfirmed) { this.service.hardDelete(item).subscribe({ next: () => { Swal.fire('تم', 'تم الحذف نهائياً', 'success'); this.load(); }, error: e => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error') }); } }); }

  onPageChange(e:any): void { this.pageSize = e.pageSize || this.pageSize; this.currentPage = (e.pageIndex != null) ? (e.pageIndex + 1) : this.currentPage; this.load(); }
}
