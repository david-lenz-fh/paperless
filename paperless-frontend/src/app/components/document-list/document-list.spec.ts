import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentList } from './document-list';

describe('DocumentList', () => {
  let component: DocumentList;
  let fixture: ComponentFixture<DocumentList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocumentList],
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
