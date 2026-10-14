import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentPreview } from './document-preview';

describe('DocumentPreview', () => {
  let component: DocumentPreview;
  let fixture: ComponentFixture<DocumentPreview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocumentPreview],
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentPreview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
