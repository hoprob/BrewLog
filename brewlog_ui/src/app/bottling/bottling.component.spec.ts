import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BottlingComponent } from './bottling.component';

describe('BottlingComponent', () => {
  let component: BottlingComponent;
  let fixture: ComponentFixture<BottlingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BottlingComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BottlingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
