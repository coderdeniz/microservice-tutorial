﻿using AutoMapper;
using FreeCourse.Services.Catalog.Dtos;
using FreeCourse.Services.Catalog.Models;
using FreeCourse.Services.Catalog.Settings;
using FreeCourse.Shared.Dtos;
using MongoDB.Driver;

namespace FreeCourse.Services.Catalog.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMapper _mapper;

        public CategoryService(IMapper mapper,IDatabaseSettings databaseSettings)
        {
            _mapper = mapper;

            var client = new MongoClient(databaseSettings.ConnectionString);
            
            var database = client.GetDatabase(databaseSettings.DatabaseName);
            
            _categoryCollection = database.GetCollection<Category>(databaseSettings.CategoryCollectionName);
        }

        public async Task<Response<List<CategoryDto>>> GetAllAsync()
        {
            var categories = await _categoryCollection.Find(category => true).ToListAsync();

            return Response<List<CategoryDto>>.Success(
                _mapper.Map<List<CategoryDto>>(categories), 
                StatusCodes.Status200OK);
        }

        public async Task<Response<CategoryDto>> CreateAsync(CategoryDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);

            await _categoryCollection.InsertOneAsync(category);

            return Response<CategoryDto>.Success(
                _mapper.Map<CategoryDto>(category),
                StatusCodes.Status201Created);
        }

        public async Task<Response<CategoryDto>> GetByIdAsync(string id)
        {
            var category = await _categoryCollection.FindAsync<Category>(
                    x => x.Id == id
                ).Result.FirstOrDefaultAsync();

            if (category == null)
            {
                return Response<CategoryDto>.Fail("Category not found", StatusCodes.Status404NotFound);
            }

            return Response<CategoryDto>.Success(
                _mapper.Map<CategoryDto>(category),
                StatusCodes.Status200OK);
        }
    }
}
