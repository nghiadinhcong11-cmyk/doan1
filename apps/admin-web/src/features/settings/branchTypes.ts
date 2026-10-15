export interface BranchManagementRecord {
  id?: string;
  name: string;
  address?: string;
  phoneNumber?: string;
  isMain: boolean;
  isActive: boolean;
  bankName?: string;
  accountNumber?: string;
  accountHolder?: string;
  imageUrl?: string;
  taxCode?: string;
  representativeName?: string;
  representativeEmail?: string;
  industry?: string;
  businessType?: string;
  createdAt?: string;
}
