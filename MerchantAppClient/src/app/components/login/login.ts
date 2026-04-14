import { DatePipe, DecimalPipe, KeyValuePipe, NgFor, NgIf, isPlatformBrowser } from '@angular/common';
import { Component, PLATFORM_ID, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MerchantApiService } from '../../core/merchant-api.service';
import { MerchantApplicationDetail, MerchantApplicationListItem, MerchantApplicationReport, PagedResponse } from '../../core/merchant-api.models';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, NgFor, NgIf, DatePipe, DecimalPipe, KeyValuePipe],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(MerchantApiService);
  private readonly platformId = inject(PLATFORM_ID);

  protected readonly statuses = signal<string[]>([]);
  protected readonly page = signal<PagedResponse<MerchantApplicationListItem> | null>(null);
  protected readonly report = signal<MerchantApplicationReport | null>(null);
  protected readonly selected = signal<MerchantApplicationDetail | null>(null);
  protected readonly loading = signal(false);
  protected readonly detailLoading = signal(false);
  protected readonly updateNote = signal('');

  protected readonly filters = this.fb.nonNullable.group({
    TaxNumber: [''],
    CompanyName: [''],
    Status: [''],
    City: [''],
    StartDate: [''],
    EndDate: [''],
    PageNumber: [1],
    PageSize: [10],
  });

  constructor() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.api.getStatuses().subscribe((value) => this.statuses.set(value));
    this.loadPage();
    this.api.getReport().subscribe((value) => this.report.set(value));
  }

  protected loadPage(pageNumber?: number): void {
    if (pageNumber) {
      this.filters.controls.PageNumber.setValue(pageNumber);
    }

    this.loading.set(true);
    this.api.getApplications(this.filters.getRawValue()).subscribe({
      next: (response) => {
        this.page.set(response);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  protected selectApplication(id: string): void {
    this.detailLoading.set(true);
    this.api.getById(id).subscribe({
      next: (response) => {
        this.selected.set(response);
        this.updateNote.set(response.statusNote ?? '');
        this.detailLoading.set(false);
      },
      error: () => this.detailLoading.set(false)
    });
  }

  protected changeStatus(status: string): void {
    const current = this.selected();
    if (!current) {
      return;
    }

    this.api.updateStatus(current.id, status, this.updateNote()).subscribe((response) => {
      this.selected.set(response);
      this.loadPage(this.filters.controls.PageNumber.value);
      this.api.getReport().subscribe((value) => this.report.set(value));
    });
  }
}
