import { useEffect, useRef, type KeyboardEvent, type ReactNode } from 'react';
import { Card } from '@/components/ui/card';

type BacklogModalProps = {
  onClose: () => void;
  children: ReactNode;
  cardClassName: string;
};

export function BacklogModal({ onClose, children, cardClassName }: BacklogModalProps) {
  const overlayRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    overlayRef.current?.focus();
  }, []);

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      event.preventDefault();
      onClose();
    }
  };

  return (
    <div
      ref={overlayRef}
      className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6"
      role="dialog"
      aria-modal="true"
      tabIndex={-1}
      onClick={onClose}
      onKeyDown={handleKeyDown}
    >
      <Card className={cardClassName} onClick={(event) => event.stopPropagation()}>
        {children}
      </Card>
    </div>
  );
}