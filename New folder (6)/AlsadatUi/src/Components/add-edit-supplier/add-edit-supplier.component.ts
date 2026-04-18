import { Component, inject, NgModule } from '@angular/core';
import { ProductService } from '../../app/Services/product.service';
import { forkJoin, Subscription } from 'rxjs';
import { ProductFilterationDto, ProductWithSupplierDto } from '../../app/models/IProductVM';
import Swal from 'sweetalert2';
import { CityServiceService } from '../../app/Services/city-service.service';
import { ICityDto, ICityFilteration } from '../../app/models/Icity';
import { SupplierService } from '../../app/Services/supplier.service';
import { FormArray, FormBuilder, FormGroup, Validators, ɵInternalFormsSharedModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions } from "@angular/material/card";
import { MatIcon } from "@angular/material/icon";
import { MatFormField, MatLabel, MatError } from "@angular/material/form-field";
import { MatSelect, MatOption } from "@angular/material/select";
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SupplierDto } from '../../app/models/ISupplierModels';

@Component({
  selector: 'app-add-edit-supplier',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
FormsModule,
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardSubtitle,
    MatCardContent,
    MatCardActions,

    MatFormField,
    MatInputModule,
    MatSelect,
    MatOption,

    MatButtonModule,
    MatIcon,
    MatTooltipModule,
    MatError
  ],
  templateUrl: './add-edit-supplier.component.html',
  styleUrl: './add-edit-supplier.component.css'
})
export class AddEditSupplierComponent {
  isExisting!: boolean;

  allProducts: ProductWithSupplierDto[] = [];
productSearchText: string[] = [];
 private ProductService = inject(ProductService);
  cities:ICityDto[]=[];
cityFilters:ICityFilteration={
  page:null,
  pageSize:null,
  cityName:null,
  governrateName:null
    }
    private supplierService = inject(SupplierService);
    private  fb = inject(FormBuilder);
    private  router = inject(Router);
    private  route = inject(ActivatedRoute);
  form!: FormGroup;
  supplierId: number | null = null;

onProductSearch(index: number, event: Event) {
  const input = event.target as HTMLInputElement;
  this.productSearchText[index] = input.value;
}


    private CityService = inject(CityServiceService);
    private CitySubscription = new Subscription();
  filtersForDeleted:ProductFilterationDto={

      isDeleted:true,
      name:null,
      page:null,
      pageSize:null
    }
    filtersForAllProoducts:ProductFilterationDto={
      
      isDeleted:null,
      name:null,
      page:null,
      pageSize:null
    }

ngOnInit(): void {
  this.IntializeForm();
  this.supplierId = +this.route.snapshot.params['id'];

  // جلب المنتجات الغير محذوفة والمنتجات المحذوفة في نفس الوقت
  forkJoin({
    all: this.ProductService.getAllProducts(this.filtersForAllProoducts),
    deleted: this.ProductService.getAllProducts(this.filtersForDeleted)
  }).subscribe({
    next: ({ all, deleted }) => {
      // تحويل لكل المنتجات
      const allProductsList = all.data.map((p: any) => ({
        productId: p.id,
        productName: p.name
      }));

      const deletedProductsList = deleted.data.map((p: any) => ({
        productId: p.id
      }));

      // الطرح: كل المنتجات اللي مش موجودة في deleted
      this.allProducts = allProductsList.filter(
        p => !deletedProductsList.some(dp => dp.productId === p.productId)
      );

      console.log('All available products:', this.allProducts);

      // بعد ما المنتجات تتحمل، نحمل المورد إذا موجود
      if (this.supplierId) {
        this.loadSupplier(this.supplierId);
      }
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'حدث خطأ',
        text: err?.error?.message,
        confirmButtonText: 'موافق'
      });
    }
  });

  this.GetAllCities();
}

createProductFormGroup(
  product?: ProductWithSupplierDto,
  isDeleted: boolean = false,
  isExisting: boolean = false
): FormGroup {
  return this.fb.group({
    productId: [product?.productId || null, Validators.required],
    productName: [product?.productName || ''],
    isDeleted: [isDeleted],
    isExisting: [isExisting]
  });
}


 get products(): FormArray {
    return this.form.get('products') as FormArray;
  }

addProduct() {
  this.products.push(this.createProductFormGroup(undefined, false, false));
  this.productSearchText.push('');
}


removeProduct(index: number) {
  this.products.removeAt(index);
  this.productSearchText.splice(index, 1);
}
getFilteredProducts(index: number): ProductWithSupplierDto[] {
  const search = (this.productSearchText[index] || '').toLowerCase();

  const selectedIds = this.products.controls
    .map((ctrl, i) => i !== index ? ctrl.get('productId')?.value : null)
    .filter(id => id !== null);

  return this.allProducts.filter(p =>
    !selectedIds.includes(p.productId) &&
    (p.productName ?? '').toLowerCase().includes(search)
  );
}


cancel() {
  this.router.navigate(['/supplier/all']);
}


     GetAllCities()
       {
        this.CitySubscription.add(this.CityService.getAllCities(this.cityFilters).subscribe({
          next:(res)=>{
            this.cities=res.data;
            console.log(res.data);

          },
          error:(err)=>{
               Swal.fire({
                                       icon: 'error',
                                       title: 'حدث خطأ',
                                       text: `${err.message}`,
                                       confirmButtonText: 'موافق',
                                       confirmButtonColor: '#d33'
                                     });
          }
        }))
       }


       IntializeForm()
       {
         this.form = this.fb.group({

      name: ['', Validators.required],
      phoneNumbers: ['', Validators.required],
      address: [''],
      cityId: [null, Validators.required],
      cityName: [''],
      isDeleted: [false],
      products: this.fb.array([])
    });
       }

loadSupplier(id: number) {
  this.supplierService.getById(id).subscribe((res) => {
    this.form.patchValue(res);

    this.products.clear();
    this.productSearchText = [];

    if (res.products) {
      res.products.forEach(p => {
        const isDeleted = !this.allProducts.some(
          ap => ap.productId === p.productId
        );

        this.products.push(
          this.createProductFormGroup(p, isDeleted, true)
        );

        this.productSearchText.push('');
      });
    }
  });
}

submit() {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }

  const formValue = this.form.value;
  console.log(formValue);

  const productsForApi: ProductWithSupplierDto[] = formValue.products.map((p: any) => ({
    productId: p.productId,
    productName: p.productName
  }));

  const supplierDto: SupplierDto = {
    id: this.supplierId ?? undefined,
    name: formValue.name,
    phoneNumbers: formValue.phoneNumbers,
    address: formValue.address,
    cityId: formValue.cityId,
    cityName: formValue.cityName,
    isDeleted: formValue.isDeleted,
    products: productsForApi
  };

  const request$ = this.supplierId
    ? this.supplierService.ditSupplier(supplierDto)
    : this.supplierService.addSupplier(supplierDto);

  request$.subscribe({
    next: () => {
      Swal.fire({
        icon: 'success',
        title: this.supplierId ? 'تم التعديل بنجاح' : 'تمت الإضافة بنجاح',
        confirmButtonText: 'موافق'
      });
     this.router.navigate(['/supplier/all']);
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'حدث خطأ',
        text: err.message,
        confirmButtonText: 'موافق'
      });
    }
  });
}



}
