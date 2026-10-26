import React, { useEffect, useState } from 'react';
import { Building2, CheckCircle2, ChevronRight, Clock3, FileText, Lock, MapPin, Package, Phone, Shield, User, Wallet } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { API_URL } from '../../../config';
import { getCachedJson } from '../../../api/queryCache';
import { Button, Feedback, FormField } from '../../../components/ui';

type ProfileTab = 'account' | 'security' | 'package' | 'billing';
type BranchProfile = { id: string; name: string; address?: string | null; phoneNumber?: string | null; imageUrl?: string | null; taxCode?: string | null; representativeName?: string | null; representativeEmail?: string | null; industry?: string | null; businessType?: string | null; isActive?: boolean };

const tabs: Array<{ id: ProfileTab; label: string; description: string; icon: React.ElementType }> = [
  { id: 'account', label: 'Hồ sơ nhà hàng', description: 'Thông tin thương hiệu và chi nhánh', icon: Building2 },
  { id: 'security', label: 'Bảo mật tài khoản', description: 'Đổi mật khẩu đăng nhập', icon: Lock },
  { id: 'package', label: 'Gói dịch vụ', description: 'Thông tin gói và quyền sử dụng', icon: Package },
  { id: 'billing', label: 'Thanh toán', description: 'Lịch sử thanh toán dịch vụ', icon: Wallet }
];

const ProfilePage = () => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<ProfileTab>('account');
  const [branch, setBranch] = useState<BranchProfile | null>(null);
  const [userName, setUserName] = useState('');
  const [userRole, setUserRole] = useState('');
  const [authUserId, setAuthUserId] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [profileError, setProfileError] = useState('');
  const [passwords, setPasswords] = useState({ current: '', next: '', confirm: '' });
  const [passwordSaving, setPasswordSaving] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  const [passwordSuccess, setPasswordSuccess] = useState(false);
  const authHeaders = { Authorization: `Bearer ${localStorage.getItem('adminToken') || ''}` };

  useEffect(() => {
    let disposed = false;
    const load = async () => {
      setIsLoading(true);
      try {
        const [meResponse, branchResponse] = await Promise.all([
          fetch(`${API_URL}/api/Auth/me`, { headers: authHeaders }),
          getCachedJson<BranchProfile[]>(`${API_URL}/api/Branch/management`, { headers: authHeaders })
        ]);
        if (!meResponse.ok) throw new Error('Không thể tải thông tin hồ sơ.');
        const me = await meResponse.json() as { userId?: string; fullName?: string; role?: string };
        const branches = branchResponse;
        const selectedId = localStorage.getItem('selectedBranchId');
        const current = branches.find(item => item.id === selectedId) || branches.find(item => item.isActive !== false) || null;
        if (!disposed) {
          setAuthUserId(me.userId || '');
          setUserName(me.fullName || localStorage.getItem('userName') || '');
          setUserRole(me.role || localStorage.getItem('userRole') || '');
          setBranch(current);
        }
      } catch (error) {
        if (!disposed) setProfileError(error instanceof Error ? error.message : 'Không thể tải thông tin hồ sơ.');
      } finally {
        if (!disposed) setIsLoading(false);
      }
    };
    void load();
    return () => { disposed = true; };
  }, []);

  const changePassword = async () => {
    setPasswordError('');
    setPasswordSuccess(false);
    if (!authUserId) return setPasswordError('Không xác định được tài khoản. Vui lòng đăng nhập lại.');
    if (passwords.next.length < 8 || passwords.next.length > 128) return setPasswordError('Mật khẩu mới phải dài từ 8 đến 128 ký tự.');
    if (passwords.next !== passwords.confirm) return setPasswordError('Mật khẩu xác nhận không khớp.');
    setPasswordSaving(true);
    try {
      const response = await fetch(`${API_URL}/api/Auth/change-password`, { method: 'POST', headers: { ...authHeaders, 'Content-Type': 'application/json' }, body: JSON.stringify({ id: authUserId, oldPassword: passwords.current, newPassword: passwords.next, type: 'Employee' }) });
      if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || 'Không thể cập nhật mật khẩu.'); }
      setPasswords({ current: '', next: '', confirm: '' });
      setPasswordSuccess(true);
    } catch (error) {
      setPasswordError(error instanceof Error ? error.message : 'Không thể kết nối máy chủ.');
    } finally { setPasswordSaving(false); }
  };

  const branchLink = userRole === 'admin' ? `/branches?branchId=${encodeURIComponent(branch?.id || '')}` : '/forbidden';
  const renderAccount = () => <div className="space-y-6">
    <div className="grid gap-5 md:grid-cols-[minmax(0,1fr)_280px]">
      <section className="rounded-2xl border border-slate-200 bg-white p-6"><div className="flex items-start justify-between gap-4"><div><p className="text-xs font-bold uppercase tracking-wider text-blue-600">Thông tin chi nhánh</p><h2 className="mt-1 text-xl font-bold text-slate-800">{branch?.name || 'Chưa chọn chi nhánh'}</h2></div><span className="inline-flex items-center gap-1 rounded-full bg-green-50 px-3 py-1 text-xs font-bold text-green-700"><CheckCircle2 size={14} />{branch?.isActive === false ? 'Tạm ngưng' : 'Đang hoạt động'}</span></div><div className="mt-6 grid gap-5 sm:grid-cols-2"><Info label="Người đại diện" value={branch?.representativeName} icon={<User size={16} />} /><Info label="Số điện thoại" value={branch?.phoneNumber} icon={<Phone size={16} />} /><Info label="Email đăng ký" value={branch?.representativeEmail} icon={<FileText size={16} />} /><Info label="Mã số thuế" value={branch?.taxCode} icon={<FileText size={16} />} /><Info label="Địa chỉ" value={branch?.address} icon={<MapPin size={16} />} /><Info label="Lĩnh vực" value={branch?.industry || branch?.businessType} icon={<Building2 size={16} />} /></div></section>
      <section className="rounded-2xl bg-gradient-to-br from-blue-600 to-blue-800 p-6 text-white shadow-lg shadow-blue-200"><div className="flex h-20 w-20 items-center justify-center overflow-hidden rounded-2xl bg-white/15 ring-1 ring-white/30">{branch?.imageUrl ? <img src={branch.imageUrl} alt="Logo chi nhánh" className="h-full w-full object-contain" /> : <Building2 size={34} className="opacity-80" />}</div><p className="mt-5 text-xs font-bold uppercase tracking-wider text-blue-100">Tài khoản đang sử dụng</p><p className="mt-1 text-lg font-bold">{userName || 'Quản trị viên'}</p><p className="mt-1 text-sm text-blue-100">Vai trò: {userRole || 'admin'}</p>{userRole === 'admin' && <button type="button" onClick={() => navigate(branchLink)} className="mt-6 inline-flex items-center gap-1 text-sm font-bold text-white hover:text-blue-100">Chỉnh sửa chi nhánh <ChevronRight size={16} /></button>}</section>
    </div>
    <section className="rounded-2xl border border-slate-200 bg-white p-6"><div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between"><div><h2 className="font-bold text-slate-800">Quản lý dữ liệu hồ sơ</h2><p className="mt-1 text-sm text-slate-500">Thông tin nhà hàng được quản lý tập trung trong phần Chi nhánh.</p></div><Button type="button" variant="outline" onClick={() => navigate(branchLink)}>Mở quản lý chi nhánh</Button></div></section>
  </div>;

  const renderSecurity = () => <div className="max-w-3xl space-y-5"><div className="grid gap-3 sm:grid-cols-3"><SecurityStatus icon={<Lock size={17} />} label="Mật khẩu" value="Đã sẵn sàng" tone="green" /><SecurityStatus icon={<Shield size={17} />} label="Xác thực 2 lớp" value="Sắp ra mắt" tone="amber" /><SecurityStatus icon={<Clock3 size={17} />} label="Phiên đăng nhập" value="Sắp ra mắt" tone="slate" /></div><div className="rounded-2xl border border-slate-200 bg-white p-6"><div className="mb-6 flex items-start gap-3"><div className="rounded-xl bg-blue-50 p-3 text-blue-600"><Shield size={20} /></div><div><h2 className="font-bold text-slate-800">Đổi mật khẩu</h2><p className="mt-1 text-sm text-slate-500">Dùng mật khẩu dài từ 8 đến 128 ký tự.</p></div></div><div className="grid gap-4 md:grid-cols-2"><FormField label="Mật khẩu hiện tại" type="password" value={passwords.current} onChange={e => setPasswords({ ...passwords, current: e.target.value })} required /><div /><FormField label="Mật khẩu mới" type="password" value={passwords.next} onChange={e => setPasswords({ ...passwords, next: e.target.value })} minLength={8} maxLength={128} required /><FormField label="Xác nhận mật khẩu mới" type="password" value={passwords.confirm} onChange={e => setPasswords({ ...passwords, confirm: e.target.value })} minLength={8} maxLength={128} required /></div>{passwordError && <div className="mt-4"><Feedback tone="error">{passwordError}</Feedback></div>}{passwordSuccess && <div className="mt-4"><Feedback tone="success">Mật khẩu đã được cập nhật.</Feedback></div>}<div className="mt-6 flex justify-end"><Button type="button" loading={passwordSaving} onClick={() => void changePassword()}>Cập nhật mật khẩu</Button></div></div></div>;

  const renderComingSoon = (tab: ProfileTab) => { const current = tabs.find(item => item.id === tab)!; const Icon = current.icon; return <div className="flex min-h-[420px] items-center justify-center rounded-3xl border border-dashed border-blue-200 bg-blue-50/50 p-8 text-center"><div className="max-w-md"><div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-white text-blue-600 shadow-sm"><Icon size={28} /></div><span className="mt-6 inline-flex rounded-full bg-amber-100 px-3 py-1 text-[11px] font-bold uppercase tracking-wider text-amber-700">Sắp ra mắt</span><h2 className="mt-4 text-xl font-bold text-slate-800">{current.label}</h2><p className="mt-2 text-sm leading-6 text-slate-500">{current.description}. Tính năng này sẽ được kết nối với hệ thống quản lý dịch vụ sau.</p></div></div>; };

  return <div className="min-h-full bg-slate-50 p-4 text-slate-800 sm:p-6 lg:p-8"><div className="mx-auto max-w-7xl"><div className="mb-6"><p className="text-xs font-bold uppercase tracking-[0.2em] text-blue-600">Tài khoản & hệ thống</p><h1 className="mt-2 text-3xl font-bold tracking-tight text-slate-900">Hồ sơ quản trị</h1><p className="mt-2 text-sm text-slate-500">Quản lý thông tin tài khoản và dữ liệu nhà hàng từ một nơi.</p></div>{profileError && <div className="mb-5"><Feedback tone="error" onDismiss={() => setProfileError('')}>{profileError}</Feedback></div>}{isLoading ? <div className="flex min-h-[420px] items-center justify-center rounded-3xl border border-slate-200 bg-white"><Clock3 className="mr-3 animate-pulse text-blue-600" size={22} />Đang tải hồ sơ...</div> : <div className="grid gap-6 lg:grid-cols-[280px_minmax(0,1fr)]"><aside className="flex gap-1 overflow-x-auto rounded-2xl border border-slate-200 bg-white p-2 shadow-sm lg:sticky lg:top-4 lg:block lg:h-fit lg:p-3"><p className="hidden px-3 pb-3 pt-2 text-xs font-bold uppercase tracking-wider text-slate-400 lg:block">Cài đặt tài khoản</p>{tabs.map(tab => { const Icon = tab.icon; return <button key={tab.id} type="button" onClick={() => setActiveTab(tab.id)} className={`flex min-w-[230px] items-center gap-3 rounded-xl px-3 py-3 text-left transition lg:min-w-0 lg:w-full ${activeTab === tab.id ? 'bg-blue-50 text-blue-700' : 'text-slate-600 hover:bg-slate-50'}`}><Icon size={18} /><span className="min-w-0 flex-1"><span className="block text-sm font-bold">{tab.label}</span><span className="mt-0.5 block truncate text-xs text-slate-400">{tab.description}</span></span><ChevronRight size={15} className={activeTab === tab.id ? 'text-blue-500' : 'text-slate-300'} /></button>; })}</aside><main>{activeTab === 'account' && renderAccount()}{activeTab === 'security' && renderSecurity()}{(activeTab === 'package' || activeTab === 'billing') && renderComingSoon(activeTab)}</main></div>}</div></div>;
};

function SecurityStatus({ icon, label, value, tone }: { icon: React.ReactNode; label: string; value: string; tone: 'green' | 'amber' | 'slate' }) { const styles = tone === 'green' ? 'border-green-200 bg-green-50 text-green-700' : tone === 'amber' ? 'border-amber-200 bg-amber-50 text-amber-700' : 'border-slate-200 bg-slate-50 text-slate-600'; return <div className={`rounded-2xl border p-4 ${styles}`}><div className="flex items-center gap-2 text-xs font-bold"><span>{icon}</span>{label}</div><p className="mt-3 text-sm font-bold">{value}</p></div>; }
function Info({ label, value, icon }: { label: string; value?: string | null; icon: React.ReactNode }) { return <div className="flex gap-3 rounded-xl border border-slate-100 bg-slate-50/70 p-3"><span className="mt-0.5 text-slate-400">{icon}</span><div className="min-w-0"><p className="text-xs font-semibold text-slate-400">{label}</p><p className="mt-1 break-words text-sm font-semibold text-slate-700">{value || 'Chưa cập nhật'}</p></div></div>; }

export default ProfilePage;
