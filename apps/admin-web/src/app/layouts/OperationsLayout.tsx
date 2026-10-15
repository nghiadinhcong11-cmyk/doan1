import { Outlet } from 'react-router-dom';

type OperationsLayoutProps = {
  mainClassName: string;
};

/** Content layout for operational routes; the session shell owns navigation. */
export default function OperationsLayout({ mainClassName }: OperationsLayoutProps) {
  return <main className={mainClassName}><Outlet /></main>;
}
