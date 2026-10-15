import React, { useState } from 'react';
import logoSource from './assets/doan-pos-logo.png';
import markSource from './assets/doan-pos-mark.png';

interface BrandLogoProps {
  className?: string;
  imageClassName?: string;
  markOnly?: boolean;
}

/**
 * Shared DOAN POS brand mark. This is intentionally independent from a
 * branch's receipt logo.
 */
const BrandLogo: React.FC<BrandLogoProps> = ({ className = '', imageClassName = '', markOnly = false }) => {
  const [imageFailed, setImageFailed] = useState(false);
  const source = markOnly ? markSource : logoSource;

  return (
    <div className={`inline-flex items-center justify-center rounded-2xl bg-white p-2 shadow-sm ring-1 ring-slate-100 ${className}`} role="img" aria-label="DOAN POS">
      {imageFailed ? (
        <span className="font-black uppercase italic tracking-tighter text-gray-800">DOAN POS</span>
      ) : (
        <img
          src={source}
          alt="DOAN POS"
          className={`block max-w-full object-contain ${imageClassName}`}
          onError={() => setImageFailed(true)}
        />
      )}
    </div>
  );
};

export default BrandLogo;
