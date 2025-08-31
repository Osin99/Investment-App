import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InvestmentList } from './investment-list.component';

describe('InvestmentList', () => {
  let component: InvestmentList;
  let fixture: ComponentFixture<InvestmentList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvestmentList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InvestmentList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
