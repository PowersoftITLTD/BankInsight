import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'indianCurrency'
})
export class IndianCurrencyPipe implements PipeTransform {
  transform(value: number | string | null): string {
    if (value === null || value === undefined || isNaN(Number(value))) {
      return '';
    }
    // Use 'en-IN' locale for Indian numbering system
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 2, // Or 0 if no decimals
      maximumFractionDigits: 2
    }).format(Number(value));
  }
}
