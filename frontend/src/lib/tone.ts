/**
 * Packaging colours. Every category bag and product bag is printed in one of them;
 * the colour is a field, never an accent. Pick by position (category rows) or by a
 * stable hash (product bags on mixed shelves) so the same bag keeps its colour.
 */
export type Tone = 'chili' | 'mango' | 'pandan' | 'cobalt'

export const TONES: Tone[] = ['chili', 'mango', 'pandan', 'cobalt']

export const toneClasses: Record<
  Tone,
  { bg: string; text: string; rim: string; muted: string; pill: string; deep: string }
> = {
  chili: {
    bg: 'bg-chili',
    text: 'text-primary-foreground',
    rim: 'text-chili',
    muted: 'text-primary-foreground/95',
    pill: 'bg-card text-chili-deep hover:bg-mango',
    deep: 'bg-chili-deep',
  },
  mango: {
    bg: 'bg-mango',
    text: 'text-foreground',
    rim: 'text-mango-deep',
    muted: 'text-foreground/85',
    pill: 'bg-card text-foreground hover:bg-chili hover:text-primary-foreground',
    deep: 'bg-mango-deep',
  },
  pandan: {
    bg: 'bg-pandan',
    text: 'text-primary-foreground',
    rim: 'text-pandan',
    muted: 'text-primary-foreground/95',
    pill: 'bg-card text-pandan-deep hover:bg-mango hover:text-foreground',
    deep: 'bg-pandan-deep',
  },
  cobalt: {
    bg: 'bg-cobalt',
    text: 'text-primary-foreground',
    rim: 'text-cobalt',
    muted: 'text-primary-foreground/95',
    pill: 'bg-card text-cobalt-deep hover:bg-mango hover:text-foreground',
    deep: 'bg-cobalt-deep',
  },
}

export function toneAt(index: number): Tone {
  return TONES[((index % TONES.length) + TONES.length) % TONES.length] as Tone
}

/** Stable colour for a bag on a mixed shelf (home page, search results). */
export function toneFor(key: string): Tone {
  let h = 0
  for (let i = 0; i < key.length; i++) h = (h * 31 + key.charCodeAt(i)) | 0
  return toneAt(Math.abs(h))
}
