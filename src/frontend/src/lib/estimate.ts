const MINUTES_PER_HOUR = 60;
const MINUTES_PER_DAY = 480; // 8h work day

/**
 * Parse a human-friendly estimate string into minutes.
 * Accepts: "10m", "2h", "1d", or a plain number (treated as minutes).
 * Returns null for empty/invalid input.
 */
export function parseEstimate(input: string): number | null {
  const trimmed = input.trim().toLowerCase();
  if (!trimmed) return null;

  const dayMatch = trimmed.match(/^(\d+(?:\.\d+)?)d$/);
  if (dayMatch) {
    const val = Math.round(parseFloat(dayMatch[1]) * MINUTES_PER_DAY);
    return val > 0 ? val : null;
  }

  const hourMatch = trimmed.match(/^(\d+(?:\.\d+)?)h$/);
  if (hourMatch) {
    const val = Math.round(parseFloat(hourMatch[1]) * MINUTES_PER_HOUR);
    return val > 0 ? val : null;
  }

  const minuteMatch = trimmed.match(/^(\d+(?:\.\d+)?)m?$/);
  if (minuteMatch) {
    const val = Math.round(parseFloat(minuteMatch[1]));
    return val > 0 ? val : null;
  }

  return null;
}

/**
 * Format a minutes value back to a human-readable estimate string.
 * e.g. 10 → "10m", 60 → "1h", 480 → "1d", 90 → "1h 30m"
 */
export function formatEstimate(minutes: number | null): string {
  if (minutes === null || minutes <= 0) return 'n/a';

  const days = Math.floor(minutes / MINUTES_PER_DAY);
  const remainAfterDays = minutes % MINUTES_PER_DAY;
  const hours = Math.floor(remainAfterDays / MINUTES_PER_HOUR);
  const mins = remainAfterDays % MINUTES_PER_HOUR;

  const parts: string[] = [];
  if (days > 0) parts.push(`${days}d`);
  if (hours > 0) parts.push(`${hours}h`);
  if (mins > 0) parts.push(`${mins}m`);

  return parts.join(' ') || 'n/a';
}

/**
 * Returns true if the estimate string is valid (parseable to a positive number).
 */
export function isValidEstimate(input: string): boolean {
  return parseEstimate(input) !== null;
}
