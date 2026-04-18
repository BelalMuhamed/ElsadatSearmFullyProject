import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface ColumnDef {
  key: string;
  label: string;
  type?: 'text' | 'number' | 'date' | 'currency' | 'boolean' | 'actions'|'calculated'|'discounted'|'string'|'copounPaiedType'|'DisOrMecrhant'|'settledStatus'|'referenceType'
}
@Component({
  selector: 'app-generic-table-component',
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule
  ],
  standalone:true,
  templateUrl: './generic-table-component.html',
  styleUrl: './generic-table-component.css'
})
export class GenericTableComponent {
@Input() data: any[] = [];
  @Input() columns: ColumnDef[] = [];
  @Input() pageSize = 10;

  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] || changes['columns']) {
      this.displayedColumns = this.columns.map(c => c.key);
      this.dataSource = new MatTableDataSource(this.data);
      queueMicrotask(() => {
        if (this.paginator) this.dataSource.paginator = this.paginator;
        if (this.sort) this.dataSource.sort = this.sort;
      });
    }
  }
}
