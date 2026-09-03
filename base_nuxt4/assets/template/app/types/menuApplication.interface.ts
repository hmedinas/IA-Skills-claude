export interface MenuTag {
    text: string;
    color?: string;
}

export interface MenuApplication {
    id: number;
    label: string;
    icon?: string;
    to?: string;
    role?: string[];
    isHeader?: boolean;
    tag?: MenuTag | null;
    children?: MenuApplication[] | null;
}
