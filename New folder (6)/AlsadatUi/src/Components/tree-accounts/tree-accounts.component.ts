// =============================================================================
// File: src/Components/tree-accounts/tree-accounts.component.ts (UPDATED)
// =============================================================================
import { Component, OnInit } from '@angular/core';
import { TreeAccountDto } from '../../app/models/TreeAccountDto';
import { TreeAccountsService } from '../../app/Services/tree-accounts.service';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-tree-accounts',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, MatTooltipModule, RouterLink],
  templateUrl: './tree-accounts.component.html',
  styleUrl: './tree-accounts.component.css'
})
export class TreeAccountsComponent implements OnInit {
  accounts: TreeAccountDto[] = [];
  flatAccounts: TreeAccountDto[] = [];
  loading = false;

  // UX additions
  searchTerm = '';
  showInactive = true;

  constructor(private treeAccountsService: TreeAccountsService, private router: Router) {}

  ngOnInit(): void { this.loadAccounts(); }

  loadAccounts(): void {
    this.loading = true;
    this.treeAccountsService.getTreeAccounts().subscribe({
      next: (data) => {
        this.accounts = data;
        this.flatAccounts = this.flattenTree(data);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  flattenTree(nodes: TreeAccountDto[] | null | undefined, level = 0): TreeAccountDto[] {
    if (!nodes?.length) return [];
    const result: TreeAccountDto[] = [];
    for (const node of nodes) {
      result.push({ ...node, level, expanded: level === 0 });
      if (node.children?.length) result.push(...this.flattenTree(node.children, level + 1));
    }
    return result;
  }

  toggleExpand(account: TreeAccountDto): void { account.expanded = !account.expanded; }

  isVisible(account: TreeAccountDto): boolean {
    // Search filter
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      const matches = account.accountName.toLowerCase().includes(term)
                   || (account.accountCode ?? '').toLowerCase().includes(term);
      if (matches) return true;
    }
    // Inactive filter
    if (!this.showInactive && !account.isActive) return false;

    if (account.level === 0) return !this.searchTerm.trim() ? true : false;
    const parent = this.findParent(account);
    if (!parent) return true;
    return !!parent.expanded && this.isVisible(parent);
  }

  findParent(account: TreeAccountDto): TreeAccountDto | null {
    const index = this.flatAccounts.indexOf(account);
    if (index <= 0) return null;
    for (let i = index - 1; i >= 0; i--) {
      if (this.flatAccounts[i].level === (account.level || 0) - 1) return this.flatAccounts[i];
    }
    return null;
  }

  hasChildren(a: TreeAccountDto): boolean { return !!a.children?.length; }

  // ✅ FIX: edit button is allowed for any non-system account that has a parent.
  // Old logic was inverted (only root accounts could be edited).
  canEdit(a: TreeAccountDto): boolean {
    return !a.isSystemAccount && a.parentId !== null && a.parentId !== undefined;
  }

  canDelete(a: TreeAccountDto): boolean {
    return !a.isSystemAccount
        && !this.hasChildren(a)
        && a.parentId !== null && a.parentId !== undefined;
  }

  canViewDetails(a: TreeAccountDto): boolean { return a.isLeaf; }

  onAdd() { this.router.navigate(['/TreeAccounts/add']); }

  onDelete(account: TreeAccountDto): void {
    if (!this.canDelete(account)) return;
    Swal.fire({
      title: 'تأكيد الحذف',
      text: `هل تريد حذف الحساب "${account.accountName}"؟`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'نعم، احذف',
      cancelButtonText: 'إلغاء'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.treeAccountsService.deleteAccount(account.id!).subscribe({
        next: r => {
          if (r.isSuccess) {
            Swal.fire({ icon: 'success', title: 'تم الحذف بنجاح', confirmButtonText: 'موافق' })
                .then(() => this.loadAccounts());
          } else {
            Swal.fire({ icon: 'error', title: 'تعذر الحذف', text: r.message ?? '', confirmButtonText: 'موافق' });
          }
        },
        error: err => Swal.fire({
          icon: 'error', title: 'خطأ',
          text: err?.error?.message ?? 'حدث خطأ غير متوقع', confirmButtonText: 'موافق'
        })
      });
    });
  }

  // Used in template
  abs(v: number): number { return Math.abs(v); }
  formatNumber(v?: number | null): string { return (v ?? 0).toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
  getBalance(a: TreeAccountDto): number { return (a.debit ?? 0) - (a.credit ?? 0); }
  getBalanceClass(b: number): string { return b > 0 ? 'balance-debit' : b < 0 ? 'balance-credit' : 'balance-zero'; }
  trackById = (_: number, a: TreeAccountDto) => a.id;
}
