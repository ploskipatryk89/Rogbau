interface Props {
  message: string | null;
  className?: string;
}

export function ErrorMessage({ message, className = 'p-4' }: Props) {
  if (!message) return null;
  return (
    <div className={`bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm ${className}`}>
      {message}
    </div>
  );
}
