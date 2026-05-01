// =============================================================================
// File: src/Components/tree-accounts/tree-accounts.component.ts
// =============================================================================
// UX:
//  - Tree starts FULLY COLLAPSED. Only the 5 root accounts are visible at first.
//  - User must expand a parent to see its children.
//  - Search auto-expands ancestor chains so matches remain visible.
//  - Toolbar exposes "Expand All" / "Collapse All" buttons.
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

  searchTerm = '';
  showInactive = true;

  constructor(private treeAccountsService: TreeAccountsService, private router: Router) {}

  ngOnInit(): void { this.loadAccounts(); }

  loadAccounts(): void {
    this.loading = true;
    this.treeAccountsService.getTreeAccounts().subscribe({
      next: (data) => {
        console.log(data);

        this.accounts = data;
        this.flatAccounts = this.flattenTree(data);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  /**
   * Flattens the tree for virtual rendering.
   * IMPORTANT: every node starts COLLAPSED (`expanded: false`). Visibility for
   * non-root rows is controlled by `isVisible()` walking the parent chain, so
   * collapsing the root nodes hides their entire subtrees.
   */
  flattenTree(nodes: TreeAccountDto[] | null | undefined, level = 0): TreeAccountDto[] {
    if (!nodes?.length) return [];
    const result: TreeAccountDto[] = [];
    for (const node of nodes) {
      result.push({ ...node, level, expanded: false }); // <-- was `level === 0`
      if (node.children?.length) {
        result.push(...this.flattenTree(node.children, level + 1));
      }
    }
    return result;
  }

  toggleExpand(account: TreeAccountDto): void {
    account.expanded = !account.expanded;
  }

  /** Expand every node that has children. */
  expandAll(): void {
    for (const a of this.flatAccounts) {
      if (this.hasChildren(a)) a.expanded = true;
    }
  }

  /** Collapse every node — only the 5 roots remain visible. */
  collapseAll(): void {
    for (const a of this.flatAccounts) {
      a.expanded = false;
    }
  }

  /**
   * Visibility rules:
   *   1) Root accounts (level 0) are ALWAYS visible (subject to inactive filter).
   *   2) Non-root accounts are visible only if their parent is visible AND expanded.
   *   3) When a search term is active, any node matching the term — and all its
   *      ancestors — auto-expand so the match is reachable.
   */
  isVisible(account: TreeAccountDto): boolean {
    if (!this.showInactive && !account.isActive) return false;

    // Root accounts always render.
    if ((account.level ?? 0) === 0) return true;

    const parent = this.findParent(account);
    if (!parent) return true;

    return !!parent.expanded && this.isVisible(parent);
  }

  /** Called by the search input (ngModelChange) to auto-expand match ancestors. */
  onSearchChange(): void {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) { return; } // user cleared search — leave tree as the user left it

    for (const node of this.flatAccounts) {
      const matches =
        node.accountName.toLowerCase().includes(term) ||
        (node.accountCode ?? '').toLowerCase().includes(term);

      if (matches) {
        // Walk up the chain and expand every ancestor.
        let parent = this.findParent(node);
        while (parent) {
          parent.expanded = true;
          parent = this.findParent(parent);
        }
      }
    }
  }

  findParent(account: TreeAccountDto): TreeAccountDto | null {
    const index = this.flatAccounts.indexOf(account);
    if (index <= 0) return null;
    const parentLevel = (account.level ?? 0) - 1;
    for (let i = index - 1; i >= 0; i--) {
      if ((this.flatAccounts[i].level ?? 0) === parentLevel) return this.flatAccounts[i];
    }
    return null;
  }

  /** Filter applied at the row level (in addition to ancestor visibility). */
  matchesSearch(account: TreeAccountDto): boolean {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return true;
    return account.accountName.toLowerCase().includes(term)
        || (account.accountCode ?? '').toLowerCase().includes(term);
  }

  /** Combined predicate used in the template. */
  shouldRender(account: TreeAccountDto): boolean {
    return this.isVisible(account) && this.matchesSearch(account);
  }

  hasChildren(a: TreeAccountDto): boolean { return !!a.children?.length; }

  canEdit(a: TreeAccountDto): boolean {
    return !a.isSystemAccount && a.parentId !== null && a.parentId !== undefined;
  }

  canDelete(a: TreeAccountDto): boolean {
    return !a.isSystemAccount
        && !this.hasChildren(a)
        && a.parentId !== null && a.parentId !== undefined;
  }

  canViewDetails(a: TreeAccountDto): boolean { return a.isLeaf; }

  onAdd(): void { this.router.navigate(['/TreeAccounts/add']); }

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

  abs(v: number): number { return Math.abs(v); }

  formatNumber(v?: number | null): string {
    return (v ?? 0).toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  getBalance(a: TreeAccountDto): number { return (a.debit ?? 0) - (a.credit ?? 0); }

  getBalanceClass(b: number): string {
    return b > 0 ? 'balance-debit' : b < 0 ? 'balance-credit' : 'balance-zero';
  }

  trackById = (_: number, a: TreeAccountDto) => a.id;
}
