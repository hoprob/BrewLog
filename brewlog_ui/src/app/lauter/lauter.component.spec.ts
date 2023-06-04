import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LauterComponent } from './lauter.component';

describe('LauterComponent', () => {
  let component: LauterComponent;
  let fixture: ComponentFixture<LauterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LauterComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LauterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
