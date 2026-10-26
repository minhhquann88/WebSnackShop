'use client'

import { Suspense, useState, type FormEvent } from 'react'
import Link from 'next/link'
import { Plus, Search } from 'lucide-react'
import { toast } from 'sonner'
import { AdminOnly } from '@/components/admin/admin-only'
import { PageHeader } from '@/components/admin/page-header'
import { useUrlFilters } from '@/components/admin/use-url-filters'
import { Badge } from '@/components/ui/badge'
import { Button, buttonVariants } from '@/components/ui/button'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Checkbox, Input, NativeSelect } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { RemoteImage } from '@/components/ui/remote-image'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import {
  useAdminCategoriesQuery,
  useAdminProductsQuery,
  useSetVisibilityMutation,
} from '@/features/admin/catalog/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatNumber, formatVnd } from '@/lib/format'

const PAGE_SIZE = 20

function ProductsList() {
  const { get, set, page } = useUrlFilters()
  const search = get('search')
  const categoryId = get('categoryId')
  const active = get('active')
  const [searchInput, setSearchInput] = useState(search)

  const categories = useAdminCategoriesQuery()
  const query = useAdminProductsQuery({
    search: search || undefined,
    categoryId: categoryId || undefined,
    isActive: active === 'true' ? true : active === 'false' ? false : undefined,
    page,
    pageSize: PAGE_SIZE,
  })
  const visibility = useSetVisibilityMutation()

  const onSearch = (e: FormEvent) => {
    e.preventDefault()
    set({ search: searchInput.trim() })
  }

  const toggle = (id: string, isActive: boolean, isFeatured: boolean) =>
    visibility.mutate(
      { id, body: { isActive, isFeatured } },
      {
        onSuccess: () => toast.success('Đã cập nhật hiển thị'),
        onError: (e) => toast.error(getErrorMessage(e)),
      },
    )

  return (
    <>
      <PageHeader
        title="Sản phẩm"
        description={query.data ? `${formatNumber(query.data.totalCount)} sản phẩm` : undefined}
        actions={
          <Link href="/admin/san-pham/moi" className={buttonVariants()}>
            <Plus aria-hidden />
            Thêm sản phẩm
          </Link>
        }
      />
      <div className="mb-4 grid gap-3 rounded-xl border bg-card p-4 sm:grid-cols-2 lg:grid-cols-4">
        <form onSubmit={onSearch} className="grid gap-1.5 sm:col-span-2">
          <Label htmlFor="product-search">Tìm kiếm</Label>
          <div className="flex gap-2">
            <Input
              id="product-search"
              placeholder="Tên sản phẩm hoặc SKU"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
            <Button type="submit" variant="outline" size="icon" aria-label="Tìm">
              <Search aria-hidden />
            </Button>
          </div>
        </form>
        <div className="grid gap-1.5">
          <Label htmlFor="product-category">Danh mục</Label>
          <NativeSelect
            id="product-category"
            value={categoryId}
            onChange={(e) => set({ categoryId: e.target.value })}
          >
            <option value="">Tất cả</option>
            {(categories.data ?? []).map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </NativeSelect>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="product-active">Trạng thái</Label>
          <NativeSelect id="product-active" value={active} onChange={(e) => set({ active: e.target.value })}>
            <option value="">Tất cả</option>
            <option value="true">Đang bán</option>
            <option value="false">Đang ẩn</option>
          </NativeSelect>
        </div>
      </div>

      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : query.data.items.length === 0 ? (
        <EmptyState title="Không có sản phẩm" />
      ) : (
        <div className="grid gap-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Sản phẩm</TableHead>
                <TableHead>Danh mục</TableHead>
                <TableHead className="text-right">Giá từ</TableHead>
                <TableHead className="text-right">Tồn bán được</TableHead>
                <TableHead className="text-right">Đã bán</TableHead>
                <TableHead>Đang bán</TableHead>
                <TableHead>Nổi bật</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data.items.map((p) => (
                <TableRow key={p.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <RemoteImage
                        src={p.imageUrl}
                        alt={p.name}
                        className="size-10 shrink-0 rounded-md border"
                      />
                      <div className="min-w-0">
                        <Link
                          href={`/admin/san-pham/${p.id}`}
                          className="font-medium text-primary hover:underline"
                        >
                          {p.name}
                        </Link>
                        <p className="text-xs text-muted-foreground">{p.variantCount} phân loại</p>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>{p.categoryName}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatVnd(p.minPrice)}</TableCell>
                  <TableCell className="text-right">
                    <Badge variant={p.totalAvailable <= 0 ? 'destructive' : 'muted'}>
                      {formatNumber(p.totalAvailable)}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right tabular-nums">{formatNumber(p.soldCount)}</TableCell>
                  <TableCell>
                    <Checkbox
                      aria-label={`Đang bán: ${p.name}`}
                      checked={p.isActive}
                      disabled={visibility.isPending}
                      onChange={(e) => toggle(p.id, e.target.checked, p.isFeatured)}
                    />
                  </TableCell>
                  <TableCell>
                    <Checkbox
                      aria-label={`Nổi bật: ${p.name}`}
                      checked={p.isFeatured}
                      disabled={visibility.isPending}
                      onChange={(e) => toggle(p.id, p.isActive, e.target.checked)}
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Pagination
            page={page}
            totalPages={totalPagesOf(query.data)}
            onPageChange={(p) => set({ page: p })}
          />
        </div>
      )}
    </>
  )
}

export default function AdminProductsPage() {
  return (
    <AdminOnly>
      <Suspense fallback={<Spinner />}>
        <ProductsList />
      </Suspense>
    </AdminOnly>
  )
}
