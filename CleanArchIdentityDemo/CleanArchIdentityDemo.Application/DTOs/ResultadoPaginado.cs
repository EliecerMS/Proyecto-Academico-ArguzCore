namespace CleanArchIdentityDemo.Application.DTOs
{
    public class ResultadoPaginado<T>
    {
        public List<T> Items { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamañoPagina { get; set; }
        public int TotalElementos { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        public ResultadoPaginado(List<T> items, int totalElementos, int numeroPagina, int tamañoPagina)
        {
            Items = items;
            TotalElementos = totalElementos;
            TamañoPagina = tamañoPagina;
            PaginaActual = numeroPagina;
            TotalPaginas = (int)Math.Ceiling(totalElementos / (double)tamañoPagina);
        }
    }
}
