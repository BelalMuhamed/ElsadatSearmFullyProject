import { HttpClientModule, provideHttpClient } from '@angular/common/http';
import { Component, Injectable } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatPaginatorIntl } from '@angular/material/paginator';

// Arabic translation for paginator - Global
@Injectable()
export class PaginatorIntlArabic extends MatPaginatorIntl {
  override itemsPerPageLabel = 'عناصر لكل صفحة:';
  override nextPageLabel = 'الصفحة التالية';
  override previousPageLabel = 'الصفحة السابقة';
  override firstPageLabel = 'الصفحة الأولى';
  override lastPageLabel = 'الصفحة الأخيرة';

  override getRangeLabel = (page: number, pageSize: number, length: number) => {
    if (length === 0 || pageSize === 0) return `0 من ${length}`;
    const startIndex = page * pageSize;
    const endIndex = Math.min(startIndex + pageSize, length);
    return `${startIndex + 1} - ${endIndex} من ${length}`;
  };
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  providers: [
    { provide: MatPaginatorIntl, useClass: PaginatorIntlArabic }
  ]
})
export class AppComponent {
  title = 'AlsadatUi';
}
