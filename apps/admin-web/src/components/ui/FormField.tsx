import React from 'react';

interface FormFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  helperText?: string;
  required?: boolean;
}

const FormField = React.forwardRef<HTMLInputElement, FormFieldProps>(function FormField(
  { label, error, helperText, required, className = '', id, ...props },
  ref
) {
  const inputId = id || `field-${label.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`;
  return (
    <div className="space-y-1.5">
      <label htmlFor={inputId} className="block text-xs font-bold text-gray-600">
        {label}{required && <span aria-hidden="true" className="ml-1 text-red-500">*</span>}
      </label>
      <input
        ref={ref}
        id={inputId}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${inputId}-error` : helperText ? `${inputId}-help` : undefined}
        className={`w-full rounded-xl border bg-white px-3 py-2.5 text-sm text-gray-700 outline-none transition-colors placeholder:text-gray-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:cursor-not-allowed disabled:bg-gray-100 ${error ? 'border-red-300 focus:border-red-500 focus:ring-red-100' : 'border-gray-200'} ${className}`}
        {...props}
      />
      {error ? <p id={`${inputId}-error`} role="alert" className="text-xs font-medium text-red-600">{error}</p> : helperText ? <p id={`${inputId}-help`} className="text-xs text-gray-500">{helperText}</p> : null}
    </div>
  );
});

export default FormField;
