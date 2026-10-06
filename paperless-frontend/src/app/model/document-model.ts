// document.model.ts

export interface DocumentDto {
  id: number;
  filename: string;
}

export interface DocumentUploadDto {
  title: string;
}

export interface FileDto {
  id?: number;
  name?: string;
  path?: string;
}

export interface DocumentUpdateDto {
  id: number;
  title: string;
}