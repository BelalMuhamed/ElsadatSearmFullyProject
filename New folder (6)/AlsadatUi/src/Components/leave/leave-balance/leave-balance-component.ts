import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { EmployeeLeaveService } from '../../../app/Services/employee-leave.service';
import { LeaveBalanceSummaryDto } from '../../../app/models/leave/leave-balance.model';
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-leave-balance',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCardModule, MatAutocompleteModule, MatIconModule, MatProgressSpinnerModule, MatTableModule],
  templateUrl: './leave-balance-component.html',
  styleUrls: ['./leave-balance-component.css']
})
export class LeaveBalanceComponent implements OnInit {
  private leaveService = inject(EmployeeLeaveService);
  summary: LeaveBalanceSummaryDto | null = null;
  year = new Date().getFullYear();
  employeeCode = localStorage.getItem('userName') || '';
  isLoading = false;
  years: number[] = [];

  ngOnInit(): void { this.load(); }

  load() {
    this.isLoading = true;
    this.leaveService.getMyLeaveBalance(this.employeeCode, this.year).subscribe({ 
      next: (r) => { this.summary = r; this.isLoading = false; }, 
      error: () => { this.summary = null; this.isLoading = false; }
    });
  }

  constructor() {
    const cur = new Date().getFullYear();
    // provide a small range of years for dropdown (editable)
    for (let y = cur - 5; y <= cur + 1; y++) this.years.push(y);
  }

  onYearInput(v: string) {
    const n = Number(v);
    if (!isNaN(n)) this.year = n;
  }

  onYearSelected(v: any) {
    const n = Number(v);
    if (!isNaN(n)) this.year = n;
  }
}
