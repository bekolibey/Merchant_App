import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { appSettings } from './app-settings';
import {
  ExchangeRateSnapshot,
  MerchantApplicationDetail,
  MerchantApplicationFilters,
  MerchantApplicationListItem,
  MerchantApplicationReport,
  PagedResponse,
} from './merchant-api.models';

@Injectable({ providedIn: 'root' })
export class MerchantApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${appSettings.apiBaseUrl}/merchant-applications`;

  getStatuses(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/reference/statuses`);
  }

  getDocumentTypes(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/reference/document-types`);
  }

  getExchangeRates(): Observable<ExchangeRateSnapshot> {
    return this.http.get<ExchangeRateSnapshot>(`${this.baseUrl}/reference/exchange-rates`);
  }

  getApplications(filters: MerchantApplicationFilters): Observable<PagedResponse<MerchantApplicationListItem>> {
    let params = new HttpParams();

    const queryFilters: Record<string, string | number | undefined> = {
      TaxNumber: filters.TaxNumber,
      WorkplaceSignboardName: filters.CompanyName,
      Status: filters.Status,
      City: filters.City,
      StartDate: filters.StartDate,
      EndDate: filters.EndDate,
      PageNumber: filters.PageNumber,
      PageSize: filters.PageSize
    };

    Object.entries(queryFilters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    });

    return this.http
      .get<BackendPagedResponse<BackendListItemResponse>>(this.baseUrl, { params })
      .pipe(
        map((response) => ({
          items: response.items.map((item) => this.mapListItem(item)),
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages
        }))
      );
  }

  getReport(): Observable<MerchantApplicationReport> {
    return this.http.get<MerchantApplicationReport>(`${this.baseUrl}/report/summary`);
  }

  getById(id: string): Observable<MerchantApplicationDetail> {
    return this.http
      .get<BackendDetailResponse>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => this.mapDetail(response)));
  }

  updateStatus(id: string, status: string, statusNote?: string): Observable<MerchantApplicationDetail> {
    return this.http
      .patch<BackendDetailResponse>(`${this.baseUrl}/${id}/status`, {
        status,
        description: statusNote ?? 'Durum guncellendi.',
        userCode: 'WEBPORTAL',
        historyDescription: statusNote ?? 'Durum bilgisi guncellendi.'
      })
      .pipe(map((response) => this.mapDetail(response)));
  }

  createApplication(payload: HomeFormPayload, documents: { type: string; file: File }[]): Observable<MerchantApplicationDetail> {
    const source = payload;
    const now = new Date();
    const nowStamp = `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}${String(now.getHours()).padStart(2, '0')}${String(now.getMinutes()).padStart(2, '0')}${String(now.getSeconds()).padStart(2, '0')}`;
    const randomSuffix = Math.floor(Math.random() * 9000) + 1000;
    const taxSuffix = source.TaxNumber.slice(-4);
    const normalizedCompanyType = source.BusinessCategory?.trim() || 'Bireysel';
    const safePhone = source.MobilePhone?.trim() || '05000000000';
    const normalizedPostalCode = this.extractPostalCode(source.AddressDetail) ?? '34000';
    const request: BackendCreateRequest = {
      applicationNumber: `APP-${nowStamp}-${randomSuffix}`,
      workflowReferenceNumber: `WF-${nowStamp}-${taxSuffix}`,
      branchCode: '0001',
      branchName: source.City ? `${source.City} Sube` : 'Merkez Sube',
      customerNumber: `CUST-${taxSuffix}${randomSuffix}`,
      identityNumber: source.AuthorizedPersonIdNumber,
      companyType: normalizedCompanyType,
      taxNumber: source.TaxNumber,
      isSpecialRulingRequired: false,
      demandDepositAccountNumber: `TR${source.TaxNumber}${randomSuffix}`,
      workplaceSignboardName: source.CompanyName,
      contractedUserCode: 'WEBPORTAL',
      tradeRegistryNumber: source.TaxNumber,
      tradeRegistryRegistrationDate: null,
      managerFullName: source.AuthorizedPersonName,
      tradeRegistryRegistrationName: source.CompanyName,
      taxOffice: source.TaxOffice,
      customerAddress: source.AddressDetail,
      posInstallationAddress: source.AddressDetail,
      email: source.Email,
      city: source.City,
      district: source.District,
      postalCode: normalizedPostalCode,
      gsmNumber: safePhone,
      latitude: Number(source.Latitude),
      longitude: Number(source.Longitude),
      webAddress: source.WebAddress || null,
      ownerIdentityNumber: source.AuthorizedPersonIdNumber,
      ownerFullName: source.AuthorizedPersonName,
      ownerMobilePhone: safePhone,
      documents: documents.map((document) => ({
        documentType: document.type,
        originalFileName: document.file.name,
        contentType: document.file.type || 'application/octet-stream',
        fileSize: document.file.size
      }))
    };

    return this.http
      .post<BackendDetailResponse>(this.baseUrl, request)
      .pipe(map((response) => this.mapDetail(response)));
  }

  private mapListItem(item: BackendListItemResponse): MerchantApplicationListItem {
    return {
      id: item.id,
      companyName: item.workplaceSignboardName,
      taxNumber: item.taxNumber,
      city: item.city,
      district: item.district,
      status: item.status,
      createdAt: item.createdAt,
      estimatedMonthlyTurnover: 0
    };
  }

  private mapDetail(response: BackendDetailResponse): MerchantApplicationDetail {
    return {
      id: response.id,
      companyName: response.workplaceSignboardName,
      taxNumber: response.taxNumber,
      taxOffice: response.taxOffice,
      authorizedPersonName: response.managerFullName,
      authorizedPersonIdNumber: response.identityNumber,
      homePhone: null,
      workPhone: null,
      mobilePhone: response.gsmNumber,
      email: response.email,
      addressDetail: response.customerAddress,
      city: response.city,
      district: response.district,
      latitude: response.latitude,
      longitude: response.longitude,
      webAddress: response.webAddress,
      businessCategory: response.companyType,
      estimatedMonthlyTurnover: 0,
      status: response.status,
      statusNote: response.histories?.[0]?.description ?? null,
      createdAt: response.createdAt,
      updatedAt: response.updatedAt ?? response.histories?.[0]?.processDate ?? null,
      exchangeRates: response.exchangeRates,
      documents: response.documents
    };
  }

  private extractPostalCode(address: string | undefined): string | null {
    if (!address) {
      return null;
    }

    const match = address.match(/\b\d{5}\b/);
    return match?.[0] ?? null;
  }
}

interface HomeFormPayload {
  CompanyName: string;
  TaxNumber: string;
  TaxOffice: string;
  AuthorizedPersonName: string;
  AuthorizedPersonIdNumber: string;
  MobilePhone: string;
  Email: string;
  AddressDetail: string;
  City: string;
  District: string;
  Latitude: number;
  Longitude: number;
  WebAddress: string;
  BusinessCategory: string;
}

interface BackendCreateRequest {
  applicationNumber: string;
  workflowReferenceNumber: string;
  branchCode: string;
  branchName: string;
  customerNumber: string;
  identityNumber: string;
  companyType: string;
  taxNumber: string;
  isSpecialRulingRequired: boolean;
  demandDepositAccountNumber: string;
  workplaceSignboardName: string;
  contractedUserCode: string;
  tradeRegistryNumber: string;
  tradeRegistryRegistrationDate: string | null;
  managerFullName: string;
  tradeRegistryRegistrationName: string;
  taxOffice: string;
  customerAddress: string;
  posInstallationAddress: string;
  email: string;
  city: string;
  district: string;
  postalCode: string;
  gsmNumber: string;
  latitude: number;
  longitude: number;
  webAddress: string | null;
  ownerIdentityNumber: string;
  ownerFullName: string;
  ownerMobilePhone: string;
  documents: BackendDocumentRequest[];
}

interface BackendDocumentRequest {
  documentType: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
}

interface BackendListItemResponse {
  id: string;
  applicationNumber: string;
  workplaceSignboardName: string;
  taxNumber: string;
  city: string;
  district: string;
  status: string;
  createdAt: string;
}

interface BackendHistoryResponse {
  id: string;
  processDate: string;
  description: string;
  status: string;
  userCode: string;
  historyDescription: string;
}

interface BackendDetailResponse {
  id: string;
  applicationNumber: string;
  workflowReferenceNumber: string;
  branchCode: string;
  branchName: string;
  customerNumber: string;
  identityNumber: string;
  companyType: string;
  taxNumber: string;
  isSpecialRulingRequired: boolean;
  demandDepositAccountNumber: string;
  workplaceSignboardName: string;
  contractedUserCode: string;
  tradeRegistryNumber: string;
  tradeRegistryRegistrationDate: string | null;
  managerFullName: string;
  tradeRegistryRegistrationName: string;
  taxOffice: string;
  customerAddress: string;
  posInstallationAddress: string;
  email: string;
  city: string;
  district: string;
  postalCode: string;
  gsmNumber: string;
  latitude: number;
  longitude: number;
  webAddress: string | null;
  ownerIdentityNumber: string;
  ownerFullName: string;
  ownerMobilePhone: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  exchangeRates: ExchangeRateSnapshot;
  documents: BackendDocumentResponse[];
  histories: BackendHistoryResponse[];
}

interface BackendDocumentResponse {
  id: string;
  documentType: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
}

interface BackendPagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
