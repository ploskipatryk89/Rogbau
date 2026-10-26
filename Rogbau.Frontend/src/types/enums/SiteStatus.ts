export const SiteStatus = {
  Planned: 1,
  InProgress: 2,
  Completed: 3,
  
} as const;

export type SiteStatus = typeof SiteStatus[keyof typeof SiteStatus];

export const SiteStatusLabels: Record<SiteStatus, string> = {
  [SiteStatus.Planned]: 'Planowana',
  [SiteStatus.InProgress]: 'W trakcie',
  [SiteStatus.Completed]: 'Zakończona',

};

export const SiteStatusColors: Record<SiteStatus, string> = {
  [SiteStatus.Planned]: 'bg-blue-50 text-blue-700 border-blue-200',
  [SiteStatus.InProgress]: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  [SiteStatus.Completed]: 'bg-slate-100 text-slate-700 border-slate-200',
  
};