export interface CopounRespDto {
  copounDesc: string;
  copounPaiedType: number; // enum should also exist in TS
  pointsToCollectCopoun: number|null
  ;
  stars: number;
  paiedCash: number;
  isActive: boolean|null;
}
