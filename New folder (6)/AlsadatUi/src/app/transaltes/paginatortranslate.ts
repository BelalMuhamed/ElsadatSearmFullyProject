import { MatPaginatorIntl } from '@angular/material/paginator';

export function getArabicPaginatorIntl(): MatPaginatorIntl {
  const paginatorIntl = new MatPaginatorIntl();

  paginatorIntl.itemsPerPageLabel = 'عناصر لكل صفحة';
  paginatorIntl.nextPageLabel = 'الصفحة التالية';
  paginatorIntl.previousPageLabel = 'الصفحة السابقة';
  paginatorIntl.firstPageLabel = 'الصفحة الأولى';
  paginatorIntl.lastPageLabel = 'الصفحة الأخيرة';
  paginatorIntl.getRangeLabel = (page: number, pageSize: number, length: number) => {
    if (length === 0 || pageSize === 0) {
      return `0 من ${length}`;
    }
    const startIndex = page * pageSize;
    const endIndex = startIndex < length
      ? Math.min(startIndex + pageSize, length)
      : startIndex + pageSize;
    return `${startIndex + 1} - ${endIndex} من ${length}`;
  };

  return paginatorIntl;
}
