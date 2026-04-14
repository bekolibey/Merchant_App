import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Component, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MerchantApiService } from '../../core/merchant-api.service';
import { appSettings } from '../../core/app-settings';
import { ExchangeRateSnapshot, MerchantApplicationDetail } from '../../core/merchant-api.models';
import { finalize } from 'rxjs';
import { NgFor, NgIf, isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [ReactiveFormsModule, NgFor, NgIf],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(MerchantApiService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly platformId = inject(PLATFORM_ID);

  protected readonly statuses = signal<string[]>([]);
  protected readonly documentTypes = signal<string[]>([]);
  protected readonly exchangeRates = signal<ExchangeRateSnapshot | null>(null);
  protected readonly selectedDocuments = signal<{ type: string; file: File }[]>([]);
  protected readonly submitting = signal(false);
  protected readonly submitResult = signal<MerchantApplicationDetail | null>(null);
  protected readonly submitError = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    CompanyName: ['', Validators.required],
    TaxNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    TaxOffice: ['', Validators.required],
    AuthorizedPersonName: ['', Validators.required],
    AuthorizedPersonIdNumber: ['', [Validators.required, Validators.pattern(/^\d{11}$/)]],
    HomePhone: [''],
    WorkPhone: [''],
    MobilePhone: ['', Validators.required],
    Email: ['', [Validators.required, Validators.email]],
    AddressDetail: ['', Validators.required],
    City: ['', Validators.required],
    District: ['', Validators.required],
    Latitude: [41.0082, Validators.required],
    Longitude: [28.9784, Validators.required],
    WebAddress: [''],
    BusinessCategory: ['', Validators.required],
    EstimatedMonthlyTurnover: [0, [Validators.required, Validators.min(1)]],
  });

  protected readonly mapUrl = computed<SafeResourceUrl>(() => {
    const latitude = this.form.controls.Latitude.value;
    const longitude = this.form.controls.Longitude.value;
    const src = `https://www.google.com/maps/embed/v1/view?key=${appSettings.googleMapsApiKey}&center=${latitude},${longitude}&zoom=15&maptype=roadmap`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(src);
  });

  constructor() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.api.getStatuses().subscribe((value) => this.statuses.set(value));
    this.api.getDocumentTypes().subscribe((value) => this.documentTypes.set(value));
    this.api.getExchangeRates().subscribe((value) => this.exchangeRates.set(value));
  }

  protected onFileSelected(event: Event, type: string): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    this.selectedDocuments.update((documents) => {
      const others = documents.filter((document) => document.type !== type);
      return [...others, { type, file }];
    });
  }

  protected submit(): void {
    this.submitError.set(null);
    this.submitResult.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.api.createApplication(this.form.getRawValue(), this.selectedDocuments())
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          this.submitResult.set(response);
          this.form.reset({
            CompanyName: '',
            TaxNumber: '',
            TaxOffice: '',
            AuthorizedPersonName: '',
            AuthorizedPersonIdNumber: '',
            HomePhone: '',
            WorkPhone: '',
            MobilePhone: '',
            Email: '',
            AddressDetail: '',
            City: '',
            District: '',
            Latitude: 41.0082,
            Longitude: 28.9784,
            WebAddress: '',
            BusinessCategory: '',
            EstimatedMonthlyTurnover: 0,
          });
          this.selectedDocuments.set([]);
        },
        error: (error) => {
          this.submitError.set(error?.error?.title ?? 'Basvuru gonderilirken bir hata olustu.');
        }
      });
  }

  protected selectedFileName(type: string): string {
    return this.selectedDocuments().find((document) => document.type === type)?.file.name ?? 'Dosya secilmedi';
  }

  protected fieldInvalid(name: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[name];
    return !!control && control.invalid && (control.touched || control.dirty);
  }
}
