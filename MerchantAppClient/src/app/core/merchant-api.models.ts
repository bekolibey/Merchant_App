export interface ExchangeRateSnapshot {
  usdToTryRate: number | null;
  eurToTryRate: number | null;
  fetchedAtUtc: string | null;
  source: string;
}

export interface MerchantDocument {
  id: string;
  documentType: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
}

export interface MerchantApplicationDetail {
  id: string;
  companyName: string;
  taxNumber: string;
  taxOffice: string;
  authorizedPersonName: string;
  authorizedPersonIdNumber: string;
  homePhone?: string | null;
  workPhone?: string | null;
  mobilePhone: string;
  email: string;
  addressDetail: string;
  city: string;
  district: string;
  latitude: number;
  longitude: number;
  webAddress?: string | null;
  businessCategory: string;
  estimatedMonthlyTurnover: number;
  status: string;
  statusNote?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  exchangeRates: ExchangeRateSnapshot;
  documents: MerchantDocument[];
}

export interface MerchantApplicationListItem {
  id: string;
  companyName: string;
  taxNumber: string;
  city: string;
  district: string;
  status: string;
  createdAt: string;
  estimatedMonthlyTurnover: number;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface MerchantApplicationReport {
  totalApplications: number;
  statusCounts: Record<string, number>;
  cityCounts: Record<string, number>;
}

export interface MerchantApplicationFilters {
  TaxNumber?: string;
  CompanyName?: string;
  Status?: string;
  City?: string;
  StartDate?: string;
  EndDate?: string;
  PageNumber?: number;
  PageSize?: number;
}
