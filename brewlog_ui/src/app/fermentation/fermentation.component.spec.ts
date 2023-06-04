import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FermentationComponent } from './fermentation.component';

describe('FermentationComponent', () => {
  let component: FermentationComponent;
  let fixture: ComponentFixture<FermentationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FermentationComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FermentationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
