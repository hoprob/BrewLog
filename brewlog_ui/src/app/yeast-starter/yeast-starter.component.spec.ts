import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YeastStarterComponent } from './yeast-starter.component';

describe('YeastStarterComponent', () => {
  let component: YeastStarterComponent;
  let fixture: ComponentFixture<YeastStarterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ YeastStarterComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(YeastStarterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
