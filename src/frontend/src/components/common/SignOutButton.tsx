import { useSetAtom } from 'jotai';
import { LogOut } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { authSessionAtom } from '@/store/authAtoms';

type SignOutButtonProps = {
  align?: 'left' | 'right';
  className?: string;
};

export function SignOutButton({ align = 'left', className }: SignOutButtonProps) {
  const setSession = useSetAtom(authSessionAtom);
  const navigate = useNavigate();

  const handleSignOut = () => {
    setSession(null);
    navigate('/', { replace: true });
  };

  return (
    <Button
      variant="outline"
      size="icon"
      onClick={handleSignOut}
      aria-label="Sign out"
      className={cn(
        'group absolute top-4 z-10 h-11 w-11 border-border/70 bg-background/85 text-foreground shadow-md backdrop-blur-sm hover:bg-background',
        align === 'right' ? 'right-4' : 'left-4',
        className,
      )}
    >
      <LogOut className="h-5 w-5" />
      <span
        className={cn(
          'pointer-events-none absolute top-1/2 -translate-y-1/2 whitespace-nowrap rounded-md border border-border/70 bg-background px-2 py-1 text-xs font-medium text-foreground opacity-0 shadow-sm transition-all duration-200 group-hover:opacity-100 group-focus-visible:opacity-100',
          align === 'right' ? 'right-full mr-2 group-hover:-translate-x-0.5' : 'left-full ml-2 group-hover:translate-x-0.5',
        )}
      >
        Sign out
      </span>
    </Button>
  );
}
