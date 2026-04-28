import type { KeyboardEvent, ReactNode } from 'react';
import { Card } from '@/components/ui/card';

type BacklogModalProps = {
  onClose: () => void;
  children: ReactNode;
  cardClassName: string;
};

export function BacklogModal({ onClose, children, cardClassName }: BacklogModalProps) {
  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      event.preventDefault();
      onClose();
    }
  };

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6" role="dialog" aria-modal="true" onClick={onClose} onKeyDown={handleKeyDown}>
      <Card className={cardClassName} onClick={(event) => event.stopPropagation()}>
        {children}
      </Card>
    </div>
  );
}