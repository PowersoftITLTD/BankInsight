import {
  Component,
  EventEmitter,
  inject,
  Input,
  Output,
  ViewChild,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OverlayPanel } from 'primeng/overlaypanel';

@Component({
  selector: 'app-advance-search-bar',
  standalone: false,
  // imports: [],
  templateUrl: './advance-search-bar.component.html',
  styleUrl: './advance-search-bar.component.scss'
})
export class AdvanceSearchBarComponent implements OnInit, OnChanges {
  @Input() buttonLabel: string = '';
  @Input() showButton: boolean = true;
  @Input() placeholderText: string = 'Search';
  @Input() filterTypeOptions: {
    type: string;
    filters: { key: string; displayName: string; Mkey?: number; propertyName: string }[];
    searchText?: string;
  }[] = [];
  @Input() singleFilterOptions: { key: string; displayName: string }[] = [];
  @Input() filterCounts: any = '';
  @Input() filterMode: 'single' | 'grouped' = 'grouped';
  @Input() immediateFilterKeys: string[] = [];
  @Input() hasSorting: boolean = false;

  @Output() onButtonClick = new EventEmitter<void>();
  @Output() searchChanged = new EventEmitter<string>();
  @Output() filterChanged = new EventEmitter<any>();
  @Output() sortFieldChanged = new EventEmitter<any>();
  @Output() sortDirectionChanged = new EventEmitter<any>();
  @Output() applyFilters = new EventEmitter<void>();

  @ViewChild('filterOverlay') filterOverlay: OverlayPanel | undefined;

  searchText: string = '';
  selectedFilters: string[] = [];
  selectedFilterType: string = '';
  activeAccordionIndexes: number = 0;
  showAccordion: boolean = true;

  route = inject(ActivatedRoute);
  router = inject(Router);

  sortFields = [
    { label: 'Creation Date', value: 'Creation_Date' },
    { label: 'Completion Date', value: 'Completion_Date' },
  ];
  selectedSortField: 'Completion_Date' | 'Creation_Date' | null = 'Completion_Date';
  sortDirection: 'asc' | 'desc' = 'asc';

  customFilterValue: number | null = null;

  @Input() searchInputWidth: string | null = null;
  @Input() showFilterButton: boolean = true;
  visibleFilters: string[] = [];
  hiddenFilters: string[] = [];
  appliedFilters: string[] = [];

  ngOnInit() {
    const durationKey = this.route.snapshot.queryParams['durationFilter'];
    const navState = window.history.state;

    if (durationKey) {
      this.selectedFilters = [durationKey];
    } else if (navState?.from !== 'dashboard') {
      this.selectedFilters = ['DEFAULT', 'ALLOCATEDBYME', 'Direct Reportee'];

      if (this.router.url.includes('task-list')) {
        const taskFiltersDropdown = JSON.parse(
          localStorage.getItem('taskFiltersDropdown') as string,
        );
        if (taskFiltersDropdown) {
          this.selectedFilters = taskFiltersDropdown;
        }
      }

      const storedCustom = localStorage.getItem('customFilterValue');
      if (storedCustom && !isNaN(+storedCustom)) {
        this.customFilterValue = +storedCustom;
      }
    }

    // Initialize appliedFilters to selectedFilters on load
    this.appliedFilters = [...this.selectedFilters];

    // ✅ Remove invalid filter keys before proceeding
    this.appliedFilters = this.appliedFilters.filter(
      (key) => this.getDisplayNameForFilter(key) !== null,
    );

    this.filterChanged.emit({ filters: this.appliedFilters, mode: this.filterMode });
    this.setupVisibleHiddenFilters();

    if (!this.selectedSortField) {
      this.selectedSortField = 'Completion_Date';
      this.sortDirection = 'asc';
      this.onSortFieldChange();
      this.sortDirectionChanged.emit(this.sortDirection);
    }

    this.syncVisibleAndHiddenFilters();
    const storedProjectFilters = localStorage.getItem('selectedProjectFilters');
    if (storedProjectFilters) {
      try {
        const saved = JSON.parse(storedProjectFilters);
        if (Array.isArray(saved)) {
          this.selectedFilters = Array.from(new Set([...this.selectedFilters, ...saved]));
          this.appliedFilters = [...this.selectedFilters];
        }
      } catch (err) {
        console.error('Invalid selectedProjectFilters in localStorage');
      }
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['filterTypeOptions'] || changes['singleFilterOptions']) {
      // Re-validate selected filters when options arrive
      this.appliedFilters = this.selectedFilters.filter(
        (key) => this.getDisplayNameForFilter(key) !== null,
      );
      this.setupVisibleHiddenFilters();
    }
  }

  onSearchChange() {
    this.searchChanged.emit(this.searchText);
  }

  isImmediateFilter(key: string): boolean {
    return this.immediateFilterKeys.includes(key);
  }

  isCustomFilterSelected(): boolean {
    return (
      this.customFilterValue !== null &&
      this.customFilterValue > 0 &&
      this.selectedFilters.includes(this.customFilterValue.toString())
    );
  }

  toggleFilterSelection(filterKey: string) {
    if (filterKey === 'custom') {
      const customKey = this.customFilterValue?.toString();
      if (this.selectedFilters.includes(customKey!)) {
        this.selectedFilters = this.selectedFilters.filter((f) => f !== customKey);
      } else if (this.customFilterValue && this.customFilterValue > 0) {
        this.selectedFilters = this.selectedFilters.filter((f) => isNaN(+f));
        this.selectedFilters.push(customKey!);
      }
      this.setupVisibleHiddenFilters();
      return;
    }

    const isProjectFilter = this.filterTypeOptions.some(
      (group) =>
        group.type.toLowerCase() === 'project' && group.filters.some((f) => f.key === filterKey),
    );

    // Toggle selection
    const index = this.selectedFilters.indexOf(filterKey);
    if (index > -1) {
      this.selectedFilters.splice(index, 1); // Uncheck
    } else {
      this.selectedFilters.push(filterKey); // Check
    }

    if (isProjectFilter) {
      this.appliedFilters = [...this.selectedFilters];

      // Save selected filters and Mkeys immediately
      localStorage.setItem('selectedProjectFilters', JSON.stringify(this.appliedFilters));

      const selectedProjectMkeys =
        this.filterTypeOptions
          .find((group) => group.type.toLowerCase() === 'project')
          ?.filters.filter((f) => this.appliedFilters.includes(f.key))
          .map((f) => f.Mkey)
          .filter((mkey) => mkey !== undefined) || [];

      localStorage.setItem('selectedProjectMkeys', JSON.stringify(selectedProjectMkeys));

      this.setupVisibleHiddenFilters();
      this.filterChanged.emit({
        filters: this.appliedFilters,
        mode: this.filterMode,
      });
      return;
    }

    if (this.isImmediateFilter(filterKey)) {
      this.selectedFilters = [filterKey];
      this.appliedFilters = [...this.selectedFilters];
      this.filterChanged.emit({ filters: this.appliedFilters, mode: this.filterMode });
      this.syncVisibleAndHiddenFilters();
      this.filterOverlay?.hide();
      return;
    }

    this.setupVisibleHiddenFilters();
  }
  applyFilter(filterKey: string) {
    this.selectedFilters = [filterKey];
    this.appliedFilters = [...this.selectedFilters];
    this.filterChanged.emit({ filters: this.appliedFilters, mode: this.filterMode });
    this.syncVisibleAndHiddenFilters();
    this.filterOverlay?.hide();
  }

  onCustomFilterValueChange(value: number | null) {
    this.customFilterValue = value;
    if (!value || value <= 0) {
      this.selectedFilters = this.selectedFilters.filter((f) => isNaN(+f));
    }
    this.setupVisibleHiddenFilters();
  }

  onOverlayOpen() {
    this.showAccordion = false;
    setTimeout(() => {
      this.activeAccordionIndexes = 0;
      this.showAccordion = true;
    });
  }

  getSelectedCountByType(type: string): number {
    const allFiltersOfType =
      this.filterTypeOptions.find((group) => group.type === type)?.filters || [];
    return allFiltersOfType.filter((f) => this.selectedFilters.includes(f.key)).length;
  }

  openFilterOverlay(event: Event, filterButton: any) {
    const hasGrouped = this.filterTypeOptions?.length > 0;
    const hasSingle = this.singleFilterOptions?.length > 0;
    const hasImmediate = this.immediateFilterKeys?.length > 0;

    if (hasGrouped || hasSingle || hasImmediate) {
      this.filterOverlay?.toggle(event, event.target as HTMLElement);
    }
  }

  toggleSortDirection() {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.sortDirectionChanged.emit(this.sortDirection);
  }

  onSortFieldChange() {
    this.sortFieldChanged.emit(this.selectedSortField);
  }

  applySelectedFilters() {
    this.appliedFilters = this.selectedFilters.filter(
      (key) => this.getDisplayNameForFilter(key) !== null,
    );
    this.filterChanged.emit({ filters: this.appliedFilters, mode: this.filterMode });
    this.applyFilters.emit();
    this.syncVisibleAndHiddenFilters();
    this.filterOverlay?.hide();
  }

  resetFilters() {
    this.selectedFilters = [];
    this.appliedFilters = [];
    this.customFilterValue = null;
    this.setupVisibleHiddenFilters();
    this.filterChanged.emit({ filters: this.selectedFilters, mode: this.filterMode });
  }

  setupVisibleHiddenFilters() {
    const MAX_VISIBLE = 2;
    const filtered = this.appliedFilters.filter(
      (key) => this.getDisplayNameForFilter(key) !== null,
    );
    this.visibleFilters = filtered.slice(0, MAX_VISIBLE);
    this.hiddenFilters = filtered.slice(MAX_VISIBLE);
  }

  syncVisibleAndHiddenFilters() {
    const MAX_VISIBLE = 2;
    const filtered = this.appliedFilters.filter(
      (key) => this.getDisplayNameForFilter(key) !== null,
    );
    this.visibleFilters = filtered.slice(0, MAX_VISIBLE);
    this.hiddenFilters = filtered.slice(MAX_VISIBLE);
  }

  getDisplayNameForFilter(key: string): string | null {
    // Grouped filters
    for (const group of this.filterTypeOptions) {
      for (const filter of group.filters) {
        if (filter.key === key) {
          return filter.displayName || key;
        }
      }
    }

    // Custom filter (numeric)
    if (!isNaN(+key) && +key > 0) {
      return `Next ${key} day(s)`;
    }

    // Single filters
    for (const filter of this.singleFilterOptions) {
      if (filter.key === key) {
        return filter.displayName || key;
      }
    }

    // Fallback
    return null;
  }

  get showBadge(): string | null {
    const groupedFilterKeys = this.filterTypeOptions.flatMap((group) =>
      group.filters.map((f) => f.key),
    );

    const singleFilterKeys = this.singleFilterOptions.map((f) => f.key);

    const validFilters = this.appliedFilters.filter((f) => {
      return (
        groupedFilterKeys.includes(f) || singleFilterKeys.includes(f) || (!isNaN(+f) && +f > 0) 
      );
    });
    return validFilters.length > 0 ? String(validFilters.length) : null;
  }
}
 {

}
