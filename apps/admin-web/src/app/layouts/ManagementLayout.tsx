import { Outlet } from 'react-router-dom';

/** Content layout for management routes; the session shell owns navigation. */
export default function ManagementLayout() {
  return <main className="h-[calc(100vh-48px)] overflow-auto lg:h-screen"><Outlet /></main>;
}
